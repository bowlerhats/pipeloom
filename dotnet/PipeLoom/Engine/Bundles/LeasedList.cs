using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using PipeLoom.Engine.Abstractions.Bundles;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Pools;

namespace PipeLoom.Engine.Bundles;

public sealed class LeasedList<T> : ILeasedList<T>, IUnsafeSpanProvider<T>, IPoolReturnable, IDisposable
{
    internal static LeasedList<T> Lease(WeaveContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        
        var pool = context.Pools.GetLeasedListPool<T>();
        var lease = pool.Lease();

        var list = lease.Item;
        
        list.Bind(context, lease);

        return list;
    }
    
    int IReadOnlyCollection<T>.Count => this.Count;

    public bool IsReadOnly => false;
    
    public long Version { get; private set; }

    public int Count { get; private set; }
    int ILeasedList<T>.Count => this.Count;
    int ICollection<T>.Count => this.Count;
    
    public T this[int index]
    {
        get => this.UnsafeGetItem(index);
        set => this.UnsafeSetItem(index, value);
    }

    public int Capacity { get; private set; }

    private readonly Lock _lock = new();
    private T[] _store = [];

    private WeaveContext _context;
    private ArrayPool<T> _pool;
    private bool _isBound;
    
    private Lease<LeasedList<T>>? _lease;
    
    internal LeasedList()
    {
        _context = null!;
        _pool = null!;
    }

    public void Dispose()
    {
        var lease = _lease;
        this.Unbind();

        if (lease.HasValue && lease.Value.IsAlive())
        {
            lease.Value.Dispose();
        }
    }

    private void Bind(WeaveContext context, Lease<LeasedList<T>> lease)
    {
        if (_isBound)
            this.Unbind();

        _lease = lease;
        _context = context;
        _pool = context.Pools.GetArrayPool<T>();

        _isBound = true;
    }

    private void Unbind()
    {
        this.Clear();
        
        _isBound = false;

        _lease = null;
        _pool = null!;
        _context = null!;
    }

    [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
    public T UnsafeGetItem(int index)
    {
        this.CheckBound();
        
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Count);

        var store = _store;
        
        var res = store[index];

        if (Volatile.Read(ref _store) != store)
            throw new InvalidOperationException("Item store of LeasedList changed");

        return res;
    }

    public void UnsafeSetItem(int index, T value)
    {
        this.CheckBound();
        
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Count);
        
        // ReSharper disable once InconsistentlySynchronizedField
        _store[index] = value;
        
        // Version is not bumped to align with vanilla List<T> behavior.
        // Because nor the underlying array nor the count did not change, it is okay to continue ongoing enumerations
    }
    
    public void Add(T item)
    {
        this.CheckBound();
        
        lock (_lock)
        {
            this.EnsureCapacity(this.Count + 1);
            _store[this.Count++] = item;
            
            this.Version++;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            if (_store.Length > 0)
            {
                _pool.Return(_store, true);
                _store = [];
            }

            this.Count = 0;
            this.Capacity = 0;
            
            this.Version++;
        }
    }

    public bool Contains(T item)
    {
        this.CheckBound();
        
        return this.IndexOf(item) >= 0;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        this.CheckBound();
        
        lock (_lock)
        {
            Array.Copy(_store, 0, array, arrayIndex, this.Count);
        }
    }

    public bool Remove(T item)
    {
        this.CheckBound();
        
        lock (_lock)
        {
            var index = Array.IndexOf(_store, item, 0, this.Count);
            if (index < 0)
                return false;
            
            this.RemoveAtInternal(index);
            
            return true;
        }
    }

    public int IndexOf(T item)
    {
        this.CheckBound();
        
        lock (_lock)
        {
            return Array.IndexOf(_store, item, 0, this.Count);
        }
    }

    public void Insert(int index, T item)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        
        this.CheckBound();
        
        lock (_lock)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Count);
            
            this.EnsureCapacity(this.Count + 1);
            
            Array.Copy(_store, index, _store, index + 1, this.Count - index);

            _store[index] = item;
            
            this.Count++;
            this.Version++;
        }
    }

    public void RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        
        this.CheckBound();
        
        lock (_lock)
        {
            this.RemoveAtInternal(index);
        }
    }

    [SuppressMessage("ReSharper", "InconsistentlySynchronizedField", Justification = "Expected to be called from an already locked context")]
    private void RemoveAtInternal(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Count);
        
        this.Count--;

        if (index < this.Count)
        {
            Array.Copy(_store, index + 1, _store, index, this.Count - index);
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            _store[this.Count] = default!;
        }

        this.Version++;
    }

    public void ReplaceItems(ReadOnlySpan<T> items)
    {
        this.CheckBound();
        
        if (items.Length == 0)
        {
            this.Clear();
            return;
        }
        
        lock (_lock)
        {
            this.EnsureCapacity(items.Length);

            this.Count = items.Length;
            items.CopyTo(_store);
            
            this.Version++;
        }
    }

    public LeasedList<T> Clone()
    {
        this.CheckBound();

        var newList = _context.Bundles.LeaseList<T>();

        lock (_lock)
        {
            newList.ReplaceItems(_store.AsSpan(0, this.Count));
        }

        return newList;
    }

    ILeasedList<T> IReadOnlyLeasedList<T>.Clone()
    {
        return this.Clone();
    }

    public LeasedListEnumerator<T> GetEnumerator()
    {
        this.CheckBound();
        
        T[] store;
        long version;
        
        lock (_lock)
        {
            store = _store;
            version = this.Version;
        }

        return new LeasedListEnumerator<T>(this, version, store);
    }

    ReturnResult IPoolReturnable.OnReturn(IObjectPool _)
    {
        this.Unbind();
        
        return ReturnResult.Ok();
    }
    
    private void EnsureCapacity(int wantedCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(wantedCapacity);
        
        if (this.Capacity >= wantedCapacity)
            return;

        wantedCapacity = Math.Max(wantedCapacity, this.Capacity * 2);
        wantedCapacity = Math.Max(wantedCapacity, MagicNumbers.MinimumLeasedListCapacity);
        
        var oldStore = _store;
        _store = _pool.Rent(wantedCapacity);
        this.Capacity = _store.Length;
        
        Array.Copy(oldStore, 0, _store, 0, this.Count);
        
        _pool.Return(oldStore, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckBound()
    {
        if (!_isBound)
            throw new PipeLoomException("LeasedList is unbound");
    }

    ReadOnlySpan<T> IUnsafeSpanProvider<T>.UnsafeAsSpan()
    {
        this.CheckBound();

        lock (_lock)
        {
            return _store.AsSpan();
        }
    }
    
    ReadOnlyMemory<T> IUnsafeSpanProvider<T>.UnsafeAsMemory()
    {
        this.CheckBound();

        lock (_lock)
        {
            return _store.AsMemory();
        }
    }
}