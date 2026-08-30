using System;
using System.Buffers;
using System.Diagnostics;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Bundles;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Pools;

namespace PipeLoom.Engine.Bundles;

internal sealed class ReadOnlyBundle<T> : IReadOnlyBundle<T>, IPoolReturnable
{
    public IWeaveContext Context => _context ?? throw new PipeLoomException("ReadOnlyBundle is unbound");
    
    // public IErasedReadOnlyBundleView Erased { get; }
    
    public ReadOnlyMemory<PartitionPath> Paths
        => (_paths ??= this.ExtractPaths()).AsMemory(0, this.PartitionCount);

    public int PartitionCount => _state is null ? 0 : _state.HasPartitions ? _state.PartitionCount : 1;
    
    public PlTypeDef ItemType { get; }
    public PlTypeDef LeafType { get; }

    private PartitionPath[]? _paths;
    private bool _isPathFromPool;

    private WeaveContext? _context;
    private ArrayPool<PartitionPath>? _pathStorePool;
    private bool _isBound;

    private BundleState<T>? _state;
    private Lease<ReadOnlyBundle<T>>? _lease;

    public ReadOnlyBundle(PipeLoomEngine engine)
    {
        this.ItemType = engine.TypeOf<T>();
        this.LeafType = engine.TypeOf<Many<T>>();
    }
    
    public void Bind(WeaveContext context, Lease<ReadOnlyBundle<T>> lease)
    {
        if (_isBound)
            this.Unbind();

        _lease = lease;
        _context = context;

        _pathStorePool = this.Context.Pools.GetArrayPool<PartitionPath>();
        
        _isBound = true;
        
        this.ResetStore();
    }

    public void Unbind()
    { 
        this.ResetStore();
        
        _isBound = false;
        
        _pathStorePool = null;
        _state = null;
        _context = null;

        if (_lease.HasValue)
        {
            var lease = _lease.Value;
            _lease = null;
            lease.Dispose();
        }
    }

    public void SetStateUnsafe(BundleState<T> state)
    {
        this.CheckBound();

        if (_state is not null)
            throw new PipeLoomException("ReadOnlyBundle already has a captured state");
        
        _state = state;
        
        Debug.Assert(_state.WasShared);
    }

    public Variant GetLeafAsPackedToVariant(PartitionPath path)
    {
        this.CheckBound();
        this.CheckState();
        
        Debug.Assert(_state != null);

        Many<T> leaf;

        if (path.IsDefault && !_state.HasPartitions)
        {
            leaf = _state.DefaultLeaf;
        } else if (_state.HasPartitions)
        {
            leaf = _state.Leafs[path];
        }
        else
        {
            throw new PipeLoomException("Invalid path reference for ReadOnlyBundle");
        }

        return Variant.From(leaf, this.LeafType);
    }
    
    public IBundle<T> Mutate()
    {
        this.CheckBound();

        if (_state is null)
            return this.Context.Bundles.Create<T>();

        // todo: get rid of referencing the internal pool
        var lease = _context!.Pools.GetBundlePool<T>().Lease();
        var res = lease.Item;
        
        res.Bind(_context!);

        res.OverrideStateUnsafe(_state);

        return res;
    }
    
    public Variant PackAsVariant()
    {
        this.CheckBound();
        
        var myType = this.Context.Engine.TypeOf<IReadOnlyBundle<T>>();
        return Variant.From(this, myType);
    }

    ReturnResult IPoolReturnable.OnReturn(IObjectPool pool)
    {
        this.Unbind();
        
        return ReturnResult.Ok();
    }

    private void CheckBound()
    {
        if (!_isBound)
            throw new PipeLoomException("ReadOnlyBundle is unbound");
    }

    private void CheckState()
    {
        if (_state == null!)
            throw new PipeLoomException("Expected valid state for ReadOnlyBundle");
    }

    private PartitionPath[] ExtractPaths()
    {
        this.CheckBound();
        
        this.ResetPath();
        
        Debug.Assert(_paths is null);
        Debug.Assert(!_isPathFromPool);
        Debug.Assert(_pathStorePool is not null);
        
        _isPathFromPool = false;
        
        if (_state is null || this.PartitionCount == 0)
        {
            return [];
        }

        if (!_state.HasPartitions)
        {
            return PartitionPath.InlineDefaultPathArray;
        }

        _isPathFromPool = true;

        var path = _pathStorePool.Rent(Math.Max(1, _state.PartitionCount));

        try
        {
            var pos = 0;
            foreach (var (partitionPath, _) in _state.Leafs)
            {
                path[pos++] = partitionPath;
            }
            
            return path;
        }
        catch (Exception)
        {
            _pathStorePool.Return(path, true);
            
            throw;
        }
    }

    private void ResetStore()
    {
        this.ResetPath();
    }

    private void ResetPath()
    {
        if (_paths is not null)
        {
            if (_isPathFromPool)
            {
                Array.Clear(_paths);
                _pathStorePool?.Return(_paths);
            }

            _paths = null;
        }
        
        _isPathFromPool = false;
    }
}