using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Threading;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine.Pools;

internal sealed class StaticPoolSet : PoolSet
{
    private readonly Lock _objectPoolFactoryLock = new();
    
    private readonly ConcurrentDictionary<Type, object> _arrayPools = [];
    private readonly ConcurrentDictionary<Type, IObjectPool> _objectPools = [];
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var (_, pool) in _objectPools)
            {
                pool.Dispose();
            }
            
            _objectPools.Clear();
            
            _arrayPools.Clear();
        }
        
        base.Dispose(disposing);
    }

    public override void ReleaseAllTouched()
    {
        foreach (var (_, pool) in _objectPools)
        {
            pool.ReleaseAll();
        }
    }

    public override ArrayPool<T> GetArrayPool<T>()
    {
        this.CheckDisposed();

        return (ArrayPool<T>)_arrayPools.GetOrAdd(typeof(T), static _ => ArrayPool<T>.Create());
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

        lock (_objectPoolFactoryLock)
        {
            if (_objectPools.TryGetValue(typeof(T), out res))
                return (IObjectPool<T>)res;
            
            res = new ObjectPool<T>(factory, maxSize);
            
            if (_objectPools.TryAdd(typeof(T), res))
                return (IObjectPool<T>)res;
            
            res.Dispose();
                
            throw new PipeLoomException("Concurrency error while creating object pool");
        }
    }
}