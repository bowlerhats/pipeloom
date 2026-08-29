using System;
using System.Diagnostics;

namespace PipeLoom.Engine.Pools;

public static class Lease
{
    public static Lease<T> Tracked<T>(long ticket, IObjectPool<T> pool)
        where T: class
    {
        return new Lease<T>(ticket, pool);
    }
    
    public static Lease<T> Untracked<T>(T item, IObjectPool<T> pool)
        where T: class
    {
        return new Lease<T>(item, pool);
    }
}

public readonly struct Lease<T>: IDisposable
    where T: class
{
    private readonly long _ticket;
    private readonly IObjectPool<T> _pool;
    private readonly T _captured;

    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public long Ticket => _ticket;
    
    public bool IsTracked => _ticket > 0;
    
    public T Item => this.IsTracked ? _pool.GetLeasedItem(_ticket) : _captured;

    internal Lease(long ticket, IObjectPool<T> pool)
    {
        Debug.Assert(ticket > 0);
        
        _ticket = ticket;
        _pool = pool;
        _captured = null!;
    }

    internal Lease(T pinned, IObjectPool<T> pool)
    {
        _ticket = -1;
        _pool = pool;
        _captured = pinned;
    }
    
    public void Dispose()
    {
        this.Release();
    }

    /// <summary>
    /// Checks if a tracked lease is alive in the pool
    /// </summary>
    /// <returns>True if it is active, false if this lease is not tracked or not present in the pool</returns>
    public bool IsAlive()
    {
        return this.IsTracked && _pool.IsLeaseActive(_ticket);
    }
    
    /// <summary>
    /// Removes lease tracking without returning the item to the pool.
    /// </summary>
    /// <remarks>
    /// Only tracked leases can be forgotten. Afterwards, <see cref="Item"/> will throw as the
    /// ticket is no longer registered in the pool.<br/>
    /// Untracked leases hold the item directly in the struct — since <see cref="Lease{T}"/> is
    /// readonly, it cannot dynamically block access to <see cref="Item"/> the same way.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the lease is not tracked.</exception>
    public void Forget()
    {
        if (!this.IsTracked)
            throw new InvalidOperationException("Cannot forget a non-tracked lease");
        
        _pool.Forget(_ticket);
    }
    
    /// <summary>
    /// Removes lease tracking and returns <see cref="Item"/> to the pool.
    /// </summary>
    public void Release()
    {
        if (this.IsTracked)
        {
            _pool.Release(_ticket);
        }
        else if (_captured is not null)
        {
            _pool.Return(_captured);
        }
    }
    
    public static implicit operator T(Lease<T> lease)
    {
        return lease.Item;
    }
}