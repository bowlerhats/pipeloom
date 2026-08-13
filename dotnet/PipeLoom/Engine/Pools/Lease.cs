using System;
using System.Diagnostics;

namespace PipeLoom.Engine.Pools;

public static class Lease
{
    public static Lease<T> Tracked<T>(long ticket, IObjectPool<T> pool)
    {
        return new Lease<T>(ticket, pool);
    }
    
    public static Lease<T> Untracked<T>(T item, IObjectPool<T> pool)
    {
        return new Lease<T>(item, pool);
    }
}

public readonly struct Lease<T>: IDisposable
{
    private readonly long _ticket;
    private readonly IObjectPool<T> _pool;
    private readonly T _captured;

    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public long Ticket => _ticket;
    
    public bool IsTracked => _ticket >= 0;
    
    public T Item => this.IsTracked ? _pool.GetLeasedItem(_ticket) : _captured;

    internal Lease(long ticket, IObjectPool<T> pool)
    {
        Debug.Assert(ticket >= 0);
        
        _ticket = ticket;
        _pool = pool;
        _captured = default!;
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

    public Lease<TAlter> As<TAlter>()
    {
        if (!typeof(TAlter).IsAssignableFrom(typeof(T)))
        {
            throw new InvalidCastException($"Lease of type '{typeof(T).Name}' cannot be assigned to requested type '{typeof(TAlter).Name}' ");
        }
        
        if (this.IsTracked)
            return new Lease<TAlter>(_ticket, (IObjectPool<TAlter>)_pool);

        return _captured is TAlter alter
            ? new Lease<TAlter>(alter, (IObjectPool<TAlter>)_pool)
            : throw new InvalidCastException($"Captured lease of type '{typeof(T).Name}' cannot be cast to requested type '{typeof(TAlter).Name}' ");
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
        else
        {
            _pool.Return(_captured);
        }
    }
    
    public static implicit operator T(Lease<T> lease)
    {
        return lease.Item;
    }
}