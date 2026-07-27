using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Channels;

namespace PipeLoom.Engine.Pools;

internal sealed class ObjectPool<T> : IObjectPool<T>
{
    private readonly Channel<T> _channel;
    private readonly Func<IObjectPool<T>, T> _factory;
    private readonly bool _isDisposable;
    private readonly Action<T> _dispose;

    private readonly Lock _unstuckLock = new();

    private readonly ConcurrentDictionary<long, T> _leasesByTicket = [];
    
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
    private readonly ConcurrentDictionary<T, long> _leasesByItem = [];
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.

    private bool _disposed;
    private long _ticket;
    
    public ObjectPool(Func<IObjectPool<T>, T> factory, int maxSize)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);
        
        _factory = factory;
        _isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T));
        _dispose = _isDisposable
            ? static item => ((IDisposable)item!).Dispose()
            : static _ => { };

        _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(maxSize)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = false,
            SingleWriter = false
        });
    }
    
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true))
            return;
        
        _channel.Writer.Complete();
        
        this.Clear(true);
    }

    public bool TryRentNoCreate([MaybeNullWhen(false)] out T rented)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        return _channel.Reader.TryRead(out rented);
    }

    public T Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (this.TryRentNoCreate(out var rented))
            return rented;

        var created = _factory(this);
        
        return created ?? throw new InvalidOperationException("Factory returned null");
    }

    public void Return(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        if (_leasesByItem.ContainsKey(obj))
            return; // actively leased, ignore
        
        if (!_channel.Writer.TryWrite(obj) && _isDisposable)
        {
            _dispose(obj);
        }
    }

    public void Clear(bool alsoLeases = false)
    {
        if (alsoLeases)
        {
            if (!_isDisposable)
            {
                _leasesByTicket.Clear();
                _leasesByItem.Clear();
            }
            else
            {
                var leasedItems = _leasesByTicket.Values.ToList();
                
                _leasesByTicket.Clear();
                _leasesByItem.Clear();

                foreach (var leasedItem in leasedItems)
                {
                    _dispose(leasedItem);
                }
            }
        }
        
        while (_channel.Reader.TryRead(out var item))
        {
            if (_isDisposable)
                _dispose(item);
        }
    }

    public Lease<T> Lease()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        var rent = this.Rent();
        try
        {
            var ticket = this.NextTicket();

            this.Track(rent, ticket);

            return Pools.Lease.Tracked(ticket, this);
        }
        catch
        {
            this.Return(rent);
            throw;
        }
    }
    
    public Lease<T> LeaseUntracked()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        return Pools.Lease.Untracked(this.Rent(), this);
    }
    
    public Lease<T>? TryLease()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        if (!this.TryRentNoCreate(out var rent))
            return null;

        try
        {
            var ticket = this.NextTicket();
            this.Track(rent, ticket);

            return Pools.Lease.Tracked(ticket, this);
        }
        catch
        {
            this.Return(rent);
            throw;
        }
    }
    
    public Lease<T>? TryLeaseUntracked()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        if (!this.TryRentNoCreate(out var rented))
            return null;

        return Pools.Lease.Untracked(rented, this);
    }

    public T GetLeasedItem(long ticket)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        return !_leasesByTicket.TryGetValue(ticket, out var res)
            ? throw new InvalidOperationException("Invalid ticket")
            : res;
    }

    public void Release(long ticket)
    {
        if (!this.UnTrack(ticket, out var removed))
            return;
        
        this.Return(removed);
    }
    
    public void Forget(long ticket)
    {
        this.UnTrack(ticket, out _);
    }

    private long NextTicket()
    {
        return Interlocked.Increment(ref _ticket);
    }

    private void Track(T rent, long ticket)
    {
        if (_disposed)
            return;
        
        if (!_leasesByItem.TryAdd(rent, ticket))
        {
            lock (_unstuckLock)
            {
                // Item already in reverse lookup — check if actively leased or stuck
                if (_leasesByItem.TryGetValue(rent, out var prevTicket))
                {
                    if (_leasesByTicket.ContainsKey(prevTicket))
                    {
                        throw new InvalidOperationException("Possible double return error detected");
                    }

                    // Stuck item — previous lease was released but reverse lookup wasn't cleaned up, reuse it
                    _leasesByItem[rent] = ticket;
                }
            }
        }
        
        _leasesByTicket[ticket] = rent;
    }

    private bool UnTrack(long ticket, [MaybeNullWhen(false)] out T removed)
    {
        if (_disposed || !_leasesByTicket.TryRemove(ticket, out removed))
        {
            removed = default!;
            return false;
        }

        if (!_leasesByItem.TryRemove(removed, out var prevTicket))
        {
            Debug.Fail("Lease tracking dicts out of sync, ticket was present, but item was not?!");
            return false;
        }

        if (ticket == prevTicket)
            return true; // all good, consistent state
        
        // oops, corrupted state detected,
        // we removed something which shouldn't be removed
        
        if (!_leasesByTicket.ContainsKey(prevTicket))
            return true; // all good, no active lease, we removed a dead track
        
        // Try to fix by readding it quickly
        if (!_leasesByItem.TryAdd(removed, prevTicket))
        {
            throw new InvalidOperationException("Failed to readd accidentally removed item");
        }

        // Here is a non-zero chance that we readded the removed item
        // but concurrently the lease holding it was released
        // and this will result in a potential double return error, which is fixable by next lease, but will
        // break pure Rents
        
        Debug.Fail("Warning: re-added item may cause transient double-return on concurrent Release.");

        return false; // reverted wrong removal, act as nothing happened
    }
}
