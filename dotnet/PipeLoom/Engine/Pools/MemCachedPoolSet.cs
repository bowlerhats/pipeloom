using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;

namespace PipeLoom.Engine.Pools;

internal sealed class MemCachedPoolSet : PoolSet
{
    private readonly Lock _objectPoolFactoryLock = new();
    
    private readonly MemoryCache _arrayPools;
    private readonly MemoryCache _objectPools;

    private readonly MemoryCacheEntryOptions _objectPoolEntryOptions;
    
    private readonly ConcurrentDictionary<IObjectPool, bool> _touched = [];
    
    public MemCachedPoolSet()
    {
        _objectPoolEntryOptions = new MemoryCacheEntryOptions();
        _objectPoolEntryOptions.RegisterPostEvictionCallback(this.ObjectPoolEvicted);
        
        _arrayPools = new MemoryCache(new MemoryCacheOptions
        {
            TrackStatistics = false,
            TrackLinkedCacheEntries = false,
            ExpirationScanFrequency = TimeSpan.FromSeconds(MagicNumbers.MemCachedPoolExpirationScanSeconds)
        });
        
        _objectPools  = new MemoryCache(new MemoryCacheOptions
        {
            TrackStatistics = false,
            TrackLinkedCacheEntries = false,
            ExpirationScanFrequency = TimeSpan.FromSeconds(MagicNumbers.MemCachedPoolExpirationScanSeconds),
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _touched.Clear();
            
            foreach (var key in _objectPools.Keys)
            {
                if (_objectPools.TryGetValue(key, out var cached) && cached is IObjectPool pool)
                {
                    // pool is multi-dispose resistant
                    pool.Dispose();
                }
            }

            _objectPools.Dispose();
            
            _arrayPools.Dispose();
        }
        
        base.Dispose(disposing);
    }

    public override void ReleaseAllTouched()
    {
        foreach (var (pool, _) in _touched)
        {
            pool.ReleaseAll();
        }
        
        _touched.Clear();
    }

    public override ArrayPool<T> GetArrayPool<T>()
    {
        this.CheckDisposed();
        
        var res = _arrayPools.Get<ArrayPool<T>>(typeof(T));
        if (res is not null)
            return res;
        
        res = ArrayPool<T>.Create();
        _arrayPools.Set(typeof(T), res);

        return res;
    }

    public override IObjectPool<T> GetObjectPool<T>(int maxSize)
    {
        return this.GetObjectPool<T>(static _ => new T(), maxSize);
    }

    public override IObjectPool<T> GetObjectPool<T>(Func<IObjectPool<T>, T> factory, int maxSize)
    {
        this.CheckDisposed();
        
        if (_objectPools.TryGetValue(typeof(T), out var cached))
        {
            var pool = (IObjectPool<T>)cached!;
            
            _touched.TryAdd(pool, true);
            
            return pool;
        }

        lock (_objectPoolFactoryLock)
        {
            if (_objectPools.TryGetValue(typeof(T), out cached))
            {
                var pool = (IObjectPool<T>)cached!;
            
                _touched.TryAdd(pool, true);
            
                return pool;
            }
            else
            {
                var pool = new ObjectPool<T>(factory, maxSize);
                _objectPools.Set(typeof(T), pool, _objectPoolEntryOptions);
            
                _touched.TryAdd(pool, true);
            
                return pool;
            }
        }
    }
    
    private void ObjectPoolEvicted(object _, object? value, EvictionReason reason, object? state)
    {
        if (value is not IObjectPool pool)
            return;
        
        _touched.TryRemove(pool, out var _);
        pool.Dispose();
    }
}