using System;
using System.Buffers;
using Microsoft.Extensions.Caching.Memory;

namespace PipeLoom.Engine.Pools;

internal sealed class MemCachedPoolSet : PoolSet
{
    private readonly IMemoryCache _arrayPools;
    private readonly IMemoryCache _objectPools;
    
    public MemCachedPoolSet(IObjectPool<MemCachedPoolSet>? origin = null)
        : base(origin is null ? null : self => origin.Return((MemCachedPoolSet)self))
    {
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
            ExpirationScanFrequency = TimeSpan.FromSeconds(MagicNumbers.MemCachedPoolExpirationScanSeconds)
        });
    }
    
    public override ArrayPool<T> GetArrayPool<T>()
    {
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
        var res = _objectPools.Get<IObjectPool<T>>(typeof(T));
        if (res is not null)
            return res;

        res = new ObjectPool<T>(factory, maxSize);
        _objectPools.Set(typeof(T), res);
        
        return res;
    }
}