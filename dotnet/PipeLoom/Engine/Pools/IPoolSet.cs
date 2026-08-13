using System;
using System.Buffers;
using System.Threading;

namespace PipeLoom.Engine.Pools;

public interface IPoolSet
{
    ArrayPool<T> GetArrayPool<T>();
}

internal abstract class PoolSet : IPoolSet, IDisposable
{
    private bool _disposed;

    public abstract void ReleaseAllTouched();
    
    public abstract ArrayPool<T> GetArrayPool<T>();
    public abstract IObjectPool<T> GetObjectPool<T>(int maxSize) where T : class, new();
    public abstract IObjectPool<T> GetObjectPool<T>(Func<IObjectPool<T>, T> factory,int maxSize);

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true))
            return;
        
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected void CheckDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}