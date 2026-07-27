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
    private readonly Action<IPoolSet>? _returnAction;

    private bool _disposed;

    protected PoolSet(Action<IPoolSet>? returnAction)
    {
        _returnAction = returnAction;
    }

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

    public void Return()
    {
        if (_returnAction is null)
        {
            this.Dispose();
        }
        else
        {
            _returnAction(this);
        }
    }

    protected void CheckDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}