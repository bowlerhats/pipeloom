using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Bundles;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Pools;

namespace PipeLoom.Engine.Bundles;

internal sealed class Bundle<T> : IBundle<T>, IPoolReturnable, ILatching<T>
{
    public WeaveContext Context => _context ?? throw new PipeLoomException("Bundle is unbound");
    IWeaveContext IBundle.Context => this.Context;
    
    public BundlePartitions<T> Partitions { get; }
    IBundlePartitions<T> IBundle<T>.Partitions => this.Partitions;
    
    public ErasedBundleView<T> Erased { get; }
    IErasedBundleView IBundle.Erased => this.Erased;
    
    public BundleItems<T> Items { get; }
    IBundleItems<T> IBundle<T>.Items => this.Items;
    
    public PipeLoomEngine Engine { get; }
    IPipeLoomEngine IBundle.Engine => this.Engine;

    public IEnumerable<PartitionPath> Paths => this.EnumeratePaths();
    
    public PlTypeDef ItemType { get; }

    public int ItemCount => _state.ItemCount;
    
    public bool IsBound => _context is not null;

    private readonly Variant[] _keyBuffer = new Variant[MagicNumbers.MaxBundlePartitionLevels];
    
    private readonly SemaphoreSlim _opLock = new(1);
    
    private SpinLock _stateLock = new(false);
    
    private WeaveContext? _context;

    private BundleState<T> _state;

    private IObjectPool<BundleState<T>>? _statePool;
    private IObjectPool<BundleState<T>> StatePool => _statePool ?? throw new PipeLoomException("Bundle is unbound");
    
    public Many<T> this[PartitionPath path]
    {
        get => this.GetLeaf(path);
        set => this.SetLeaf(path, value);
    }

    public Bundle(PipeLoomEngine engine)
    {
        this.Engine = engine;
        this.ItemType = engine.TypeOf<T>();
        
        this.Partitions = new BundlePartitions<T>(this);
        this.Erased = new ErasedBundleView<T>(this);
        this.Items = new BundleItems<T>(this);

        _state = null!;
    }

    public void Bind(WeaveContext context)
    {
        if (_context is not null)
            this.Unbind();

        if (context.Engine != this.Engine)
            throw new PipeLoomException("Cannot bind Bundle to another PipeLoom engine!");
        
        _context = context;
        
        _statePool = context.Pools.GetBundleStatePool<T>();
        
        var lease = this.StatePool.Lease();
        _state = lease.Item;
        _state.Lease = lease;
        _state.Bind(this.Context);

    }

    public void Unbind()
    {
        
        _statePool = null!;
        
        _context = null;
        _state = null!;
    }
    
    public ReturnResult OnReturn(IObjectPool pool)
    {
        this.Unbind();
        
        // todo: protect against id and version overflows, request drop if near limits
        return ReturnResult.Ok();
    }
    
    public BundleReadLatch<T> Latch()
    {
        BundleState<T> state;
        
        var lockTaken = false;
        _stateLock.Enter(ref lockTaken);
        try
        {
            state = _state;
            _state.IsMutating = false;
            _state.ActiveLatches++;
        }
        finally
        {
            if (lockTaken)
                _stateLock.Exit(false);
        }
        
        return new BundleReadLatch<T>(state, this);
    }

    public void LatchRelease(BundleState<T> state)
    {
        bool canReturnState;
        var lockTaken = false;
        _stateLock.Enter(ref lockTaken);
        try
        {
            _state.IsMutating = false;
            if (state.ActiveLatches > 0)
                state.ActiveLatches--;

            canReturnState = !state.WasShared && state.Version != _state.Version && state.ActiveLatches == 0;
        }
        finally
        {
            if (lockTaken)
                _stateLock.Exit(false);
        }

        if (canReturnState)
        {
            this.ReturnState(state);
        }
    }

    public Many<T> GetLeaf(PartitionPath path)
    {
        this.CheckBound();
        
        using var latch = this.Latch();

        return latch.State.Leafs[path];
    }

    public Many<T> SetLeaf(PartitionPath path, Many<T> newLeaf)
    {
        this.CheckBound();
        
        var oldLeaf = newLeaf;

        if (!_opLock.Wait(MagicNumbers.BundleOpLockWaitTime))
            throw new PipeLoomException("OpLock timed out");
        try
        {
            _state.CheckValidPathForState(path);
            
            var done = false;
            do
            {
                var settled = this.Mutating();
                
                settled.CheckValidPathForState(path);

                var lockTaken = false;
                _stateLock.Enter(ref lockTaken);
                try
                {
                    if (_state.IsMutating && _state.Version == settled.Version)
                    {
                        if (_state.HasPartitions)
                        {
                            if (!_state.Leafs.TryGetValue(path, out oldLeaf))
                            {
                                oldLeaf = Many<T>.Empty;
                            }

                            _state.Leafs[path] = newLeaf;
                        }
                        else
                        {
                            oldLeaf = _state.DefaultLeaf;
                            _state.DefaultLeaf = newLeaf;
                        }
                        
                        _state.ItemCount -= oldLeaf.Length;
                        _state.ItemCount += newLeaf.Length;

                        done = true;
                    }
                }
                finally
                {
                    if (lockTaken)
                        _stateLock.Exit(true);
                }

            } while (!done);
        }
        finally
        {
            _opLock.Release();
        }

        return oldLeaf;
    }

    public Many<T> SetLeaf(PartitionPath path, T singularLeaf)
    {
        return this.SetLeaf(path, Many.Single(singularLeaf));
    }

    public bool Repartition(bool allowCollapse = false)
    {
        this.CheckBound();
        throw new NotImplementedException();
    }

    public ValueTask<bool> RepartitionAsync(bool allowCollapse = false)
    {
        this.CheckBound();
        throw new NotImplementedException();
    }

    public Variant PackAsVariant()
    {
        this.CheckBound();
        return Variant.From(this, this.Context.Engine);
    }

    public IReadOnlyBundle<T> AsReadOnly()
    {
        this.CheckBound();
        
        throw new NotImplementedException();
    }

    public Many<T> Flatten()
    {
        this.CheckBound();

        using var latch = this.Latch();

        if (!latch.State.HasPartitions)
            return latch.State.DefaultLeaf;
        
        // defensive recount of items to ensure buffer bounds
        latch.State.RecountItems();

        var itemCount = latch.State.ItemCount;

        var pool = this.Context.Pools.GetArrayPool<T>();
        var buffer = pool.Rent(itemCount);
        try
        {
            var pos = 0;
            foreach (var leaf in latch.State.Leafs.Values)
            {
                switch (leaf.Length)
                {
                    case 0: break;
                    case 1:
                        buffer[pos++] = leaf[0];
                        break;
                    default:
                        leaf.AsSpan().CopyTo(buffer.AsSpan(pos, leaf.Length));
                        pos += leaf.Length;
                        break;
                }
            }
            
            Debug.Assert(pos == itemCount);
            
            return Many.Create(buffer.AsSpan(0, itemCount));
        }
        finally
        {
            pool.Return(buffer);
        }
    }
    
    public bool ContainsPartition(params ReadOnlySpan<Variant> keys)
    {
        this.CheckBound();
        
        using var latch = this.Latch();
        var state = latch.State;

        if (!state.HasPartitions || keys.Length != state.LevelCount || state.PartitionCount == 0)
            return false;

        var lookup = state.Leafs.GetAlternateLookup<ReadOnlySpan<Variant>>();
        return lookup.ContainsKey(keys);
    }

    public IEnumerable<PartitionPath> EnumeratePaths()
    {
        foreach (var partition in this.Partitions)
        {
            yield return partition.Path;
        }
    }

    public void Add(T item)
    {
        this.CheckBound();
        
        if (!this.TryAdd(item))
            throw new PipeLoomException("Cannot add item to bundle. Partitioning problem ?!");
    }

    public bool TryAdd(T item)
    {
        this.CheckBound();
        
        if (!_opLock.Wait(MagicNumbers.BundleOpLockWaitTime))
            throw new PipeLoomException("OpLock timed out");
        try
        {
            var vTask = this.TryAddAsyncCore(item);
        
            return vTask.IsCompletedSuccessfully
                ? vTask.Result
                : vTask.AsTask().GetAwaiter().GetResult();
        }
        finally
        {
            _opLock.Release();
        }
    }

    public async ValueTask AddAsync(T item)
    {
        this.CheckBound();
        
        if (!await this.TryAddAsync(item))
            throw new PipeLoomException("Cannot add item to bundle. Partitioning problem ?!");
    }

    public async ValueTask<bool> TryAddAsync(T item)
    {
        this.CheckBound();
        
        if (!await _opLock.WaitAsync(MagicNumbers.BundleOpLockWaitTime))
            throw new PipeLoomException("OpLock timed out");
        try
        {
            return await this.TryAddAsyncCore(item);
        }
        finally
        {
            _opLock.Release();
        }
    }

    private async ValueTask<bool> TryAddAsyncCore(T item)
    {
        var done = false;
        do
        {
            var settled = this.Mutating();

            if (!settled.HasPartitions)
            {
                var newLeaf = settled.DefaultLeaf.Add(item);

                var lockTaken = false;
                _stateLock.Enter(ref lockTaken);
                try
                {
                    if (_state.IsMutating && settled.Version == _state.Version)
                    {
                        _state.DefaultLeaf = newLeaf;
                        _state.ItemCount++;

                        done = true;
                    }
                }
                finally
                {
                    if (lockTaken)
                        _stateLock.Exit(false);
                }
            }
            else
            {
                var canAutoPartition = settled.CanAutoPartition();
                if (!canAutoPartition)
                    return false;

                var vItem = Variant.From(item, this.ItemType);

                Array.Clear(_keyBuffer);
                var keyCount = await settled.ParsePartitionKeysAsync(vItem, _keyBuffer);
                if (keyCount != settled.LevelCount)
                    return false;

                var pathLease = this.Context.Pools.PartitionPaths.Lease();
                var path = pathLease.Item;
                path.SetKeys(_keyBuffer.AsSpan(0, keyCount));

                Many<T> newLeaf;

                var pathCaptured = false;
                if (settled.Leafs.TryGetValue(path, out var leaf))
                {
                    newLeaf = leaf.Add(item);
                }
                else
                {
                    pathCaptured = true;
                    newLeaf = Many<T>.Empty.Add(item);
                }

                var lockTaken = false;
                _stateLock.Enter(ref lockTaken);
                try
                {
                    if (_state.IsMutating && settled.Version == _state.Version)
                    {
                        _state.Leafs[path] = newLeaf;
                        _state.ItemCount++;

                        done = true;
                    }
                }
                finally
                {
                    if (lockTaken)
                        _stateLock.Exit(false);
                }

                if (!done || !pathCaptured)
                {
                    pathLease.Dispose();
                }
            }
        } while (!done);

        return true;
    }

    public BundleState<T> Mutating()
    {
        // assumes was called from an oplock, so no other real mutation is in progress
        BundleState<T> settled;
        bool needsCopy;
        do
        {
            BundleState<T> currentState;
            
            var lockTaken = false;
            _stateLock.Enter(ref lockTaken);
            try
            {
                currentState = _state;
                needsCopy = !_state.IsMutating && (_state.WasShared || _state.ActiveLatches > 0);
            }
            finally
            {
                if (lockTaken)
                    _stateLock.Exit(false);
            }
            
            settled = currentState;

            if (!needsCopy)
                break;

            var newState = this.DeriveNewState(currentState, forMutating: true);

            lockTaken = false;
            _stateLock.Enter(ref lockTaken);
            try
            {
                if (_state.Version == currentState.Version)
                {
                    _state = newState;
                    settled = newState;
                    newState = null;
                    needsCopy = false;
                }
                else
                {
                    needsCopy = !_state.IsMutating && (_state.WasShared || _state.ActiveLatches > 0);
                }
            }
            finally
            {
                if (lockTaken)
                    _stateLock.Exit(false);
            }

            if (newState is not null)
            {
                this.ReturnState(newState);
            }
            
        } while (needsCopy);

        return settled;
    }

    public IBundle<TOther> ConvertTo<TOther>(Func<T, TOther> converterFunc)
    {
        return this.ConvertTo(converterFunc, static (func, d) => func(d));
    }

    public IBundle<TOther> ConvertTo<TOther, TState>(TState state, Func<TState, T, TOther> converterFunc)
    {
        var res = this.Context.Bundles.Create<TOther>();
        
        foreach (var partition in this.Partitions)
        {
            var cLeaf = partition.Leaf.ConvertTo(this.Context, state, converterFunc);
            res.SetLeaf(partition.Path, cLeaf);
        }

        return res;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CheckBound()
    {
        if (!this.IsBound)
            throw new PipeLoomException("Bundle is unbound");
    }

    private BundleState<T> DeriveNewState(BundleState<T> currentState, bool forMutating)
    {
        this.CheckBound();
        
        var lease = this.StatePool.Lease();
        var state = lease.Item;
        state.Lease = lease;
        
        state.Bind(this.Context);
        state.DeriveFrom(currentState);
        
        state.IsMutating = forMutating;

        return state;
    }

    private void ReturnState(BundleState<T> state)
    {
        if (state.WasShared)
            return;

        var lease = state.Lease;
        state.Unbind();
        
        if (lease.HasValue)
        {
            lease.Value.Dispose();
        }
        else
        {
            this.StatePool.Return(state);
        }
    }
}