using System;
using System.Buffers;
using System.Threading;
using PipeLoom.Engine.Abstractions.Bundles;
using PipeLoom.Engine.Abstractions.Bundles.ListSources;
using PipeLoom.Engine.Bundles;

namespace PipeLoom.Engine.Pools;

public interface IPoolSet
{
    ArrayPool<T> GetArrayPool<T>();
}

internal abstract class PoolSet : IPoolSet, IDisposable
{
    public PipeLoomEngine Engine { get; }
    
    public IObjectPool<BundleFactory> BundleFactories
    {
        get => field is null || field.IsDisposed
            ? field = this.GetObjectPool<BundleFactory>(MagicNumbers.BundleFactoryPoolSize)
            : field;
        protected set;
    }

    public IObjectPool<StepState> StepStates
    {
        get => field is null || field.IsDisposed
            ? field = this.GetObjectPool<StepState>(MagicNumbers.StepStatePoolSize)
            : field;
        protected set;
    }
    
    public IObjectPool<PartitionPath> PartitionPaths
    {
        get => field is null || field.IsDisposed
            ? field = this.GetObjectPool<PartitionPath>(
                static _ => new PartitionPath(),
                MagicNumbers.PartitionPathPoolSize)
            : field;
        protected set;
    }
    
    private bool _disposed;

    protected PoolSet(PipeLoomEngine engine)
    {
        this.Engine = engine;
    }
    
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true))
            return;
        
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    public abstract void ReleaseAllTouched();
    
    public abstract ArrayPool<T> GetArrayPool<T>();
    public abstract IObjectPool<T> GetObjectPool<T>(int maxSize) where T : class, new();
    public abstract IObjectPool<T> GetObjectPool<T>(Func<IObjectPool<T>, T> factory,int maxSize) where T: class;
    public abstract IObjectPool<T> GetObjectPool<T, TState>(TState state, Func<TState, IObjectPool<T>, T> factory,int maxSize) where T: class;

    protected virtual void OnObjectPoolEvicted(IObjectPool evicted)
    {
        this.BundleFactories = HandleEviction(this.BundleFactories, evicted);
        this.StepStates = HandleEviction(this.StepStates, evicted);
        this.PartitionPaths = HandleEviction(this.PartitionPaths, evicted);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public IObjectPool<Bundle<T>> GetBundlePool<T>()
    {
        return this.GetObjectPool<Bundle<T>, PipeLoomEngine>(this.Engine,
            static (engine, _) => new Bundle<T>(engine),
            MagicNumbers.BundlePoolSize);
    }

    public IObjectPool<LeasedList<T>> GetLeasedListPool<T>()
    {
        return this.GetObjectPool<LeasedList<T>>(static _ => [], MagicNumbers.LeasedListPoolSize);
    }
    
    public IObjectPool<BundleState<T>> GetBundleStatePool<T>()
    {
        return this.GetObjectPool<BundleState<T>>(MagicNumbers.BundleStatePoolSize);
    }

    public IObjectPool<ReadOnlyBundle<T>> GetReadOnlyBundlePool<T>()
    {
        return this.GetObjectPool<ReadOnlyBundle<T>, PipeLoomEngine>(
            this.Engine,
            static (engine, _) => new ReadOnlyBundle<T>(engine),
            MagicNumbers.ReadOnlyBundlePoolSize);
    }
    
    protected void CheckDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private static TPool HandleEviction<TPool>(TPool candidate, IObjectPool evicted)
        where TPool : IObjectPool
    {
        return candidate.IsDisposed || ReferenceEquals(candidate, evicted) ? default! : candidate;
    }
}