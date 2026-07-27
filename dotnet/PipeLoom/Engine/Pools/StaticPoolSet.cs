using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine.Pools;

internal sealed class StaticPoolSet : PoolSet
{
    private readonly ConcurrentDictionary<Type, object> _arrayPools = [];
    private readonly ConcurrentDictionary<Type, object> _objectPools = [];
    
    public StaticPoolSet(IObjectPool<StaticPoolSet>? origin = null)
        : base(origin is null ? null : self => origin.Return((StaticPoolSet)self))
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _arrayPools.Clear();
            
            foreach (var pool in _objectPools.Values)
            {
                if (pool is IDisposable disposable)
                    disposable.Dispose();
            }
            
            _objectPools.Clear();
        }
        
        base.Dispose(disposing);
    }

    public override ArrayPool<T> GetArrayPool<T>()
    {
        this.CheckDisposed();
        
        if (_arrayPools.TryGetValue(typeof(T), out var res))
            return (ArrayPool<T>)res;
        
        res = ArrayPool<T>.Create();
        if (!_arrayPools.TryAdd(typeof(T), res))
        {
            res = _arrayPools.GetValueOrDefault(typeof(T))
                  ?? throw new PipeLoomException("Bad pool set");
        }

        return (ArrayPool<T>)res;
    }

    public override IObjectPool<T> GetObjectPool<T>(int maxSize)
    {
        return this.GetObjectPool<T>(static _ => new T(), maxSize);
    }
    
    public override IObjectPool<T> GetObjectPool<T>(Func<IObjectPool<T>, T> factory, int maxSize)
    {
        this.CheckDisposed();
        
        if (_objectPools.TryGetValue(typeof(T), out var res))
            return (IObjectPool<T>)res;
        
        res = new ObjectPool<T>(factory, maxSize);
        if (!_objectPools.TryAdd(typeof(T), res))
        {
            res = _objectPools.GetValueOrDefault(typeof(T))
                  ?? throw new PipeLoomException("Bad pool set");
        }
        
        return (IObjectPool<T>)res;
    }
}