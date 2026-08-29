using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Bundles;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Pools;

namespace PipeLoom.Engine.Bundles;

internal sealed class BundleState<T> : IPoolReturnable
{
    public Lease<BundleState<T>>? Lease;
    
    public bool IsMutating;

    public long Version;

    public bool WasShared;

    public int ActiveLatches;

    public readonly PartitionLevel[] Levels = new PartitionLevel[MagicNumbers.MaxBundlePartitionLevels];
    public byte LevelCount;

    // todo: optimize working with pooled arrays instead of dictionary of leafs
    public Dictionary<PartitionPath, Many<T>> Leafs = new(PartitionPath.BoundPathComparer.Instance);

    public int PartitionCount => Leafs.Count;
    public int ItemCount;

    public Many<T> DefaultLeaf = Many<T>.Empty;

    private WeaveContext? _context;
    private WeaveContext Context => _context ?? throw new PipeLoomException("Bundle state is unbound");

    public ReadOnlySpan<PartitionLevel> ActiveLevels => Levels.AsSpan(0, LevelCount);

    public bool HasPartitions => LevelCount > 0;
    
    public void Bind(WeaveContext context)
    {
        Debug.Assert(_context is null);
        
        if (_context is not null)
            this.Unbind();
        
        _context = context;
        
        if (Leafs.Count > 0)
            Leafs = new Dictionary<PartitionPath, Many<T>>(PartitionPath.BoundPathComparer.Instance);
        
        ItemCount = 0;
        LevelCount = 0;
        Array.Clear(Levels);
        DefaultLeaf = Many<T>.Empty;
        WasShared = false;
        IsMutating = false;
        ActiveLatches = 0;
        Lease = null;
    }

    public void Unbind()
    {
        this.AssertUnboundState();
        
        _context = null;
        
        if (Leafs.Count > 0)
            Leafs = new Dictionary<PartitionPath, Many<T>>(PartitionPath.BoundPathComparer.Instance);
        
        ItemCount = 0;
        LevelCount = 0;
        Array.Clear(Levels);
        DefaultLeaf = Many<T>.Empty;
        WasShared = false;
        IsMutating = false;
        ActiveLatches = 0;
        
        Lease = null;
    }

    public void DeriveFrom(BundleState<T> parent)
    {
        Version = parent.Version + 1;
        LevelCount = parent.LevelCount;
        
        if (LevelCount == 0)
        {
            DefaultLeaf = parent.DefaultLeaf;
        }
        else
        {
            parent.ActiveLevels.CopyTo(Levels);
            Leafs = new Dictionary<PartitionPath, Many<T>>(parent.Leafs, PartitionPath.BoundPathComparer.Instance);
        }
        
        this.RecountItems();
    }

    public ReturnResult OnReturn(IObjectPool pool)
    {
        this.Unbind();
        
        return ReturnResult.Ok();
    }

    public void RecountItems()
    {
        ItemCount = LevelCount == 0
            ? DefaultLeaf.Length
            : Leafs.Values.Sum(d => d.Length);
    }

    public void CheckValidPathForState(PartitionPath path)
    {
        var levels = Volatile.Read(ref LevelCount);
        switch (path.IsDefault)
        {
            case true when levels > 0:
                throw new PipeLoomException("Default path is invalid for partitioned bundles");
            case false when levels == 0:
                throw new PipeLoomException("Unpartitioned bundle expects default path");
        }
    }

    public bool CanAutoPartition()
    {
        if (LevelCount == 0)
            return true;
        
        for (var i = 0; i < LevelCount; i++)
        {
            if (Levels[i].Partitioner is null)
                return false;
        }

        return true;
    }

    public async ValueTask<int> ParsePartitionKeysAsync(Variant item, Variant[] keyBuffer)
    {
        var levels = LevelCount;
        for (var i = 0; i < levels; i++)
        {
            var level = Levels[i];
            if (level.Partitioner is null)
                return i;

            keyBuffer[i] = level.Partitioner.IsAsync
                ? await level.Partitioner.GetKeyAsync(item, this.Context)
                : level.Partitioner.GetKey(item, this.Context);
        }

        return levels;
    }

    [Conditional("DEBUG")]
    private void AssertUnboundState()
    {
        if (_context is not null)
            return; // normal unbind
        
        // Otherwise can only be a double unbind
        // Assert that state is "pristine"
        Debug.Assert(ItemCount == 0);
        Debug.Assert(LevelCount == 0);
        Debug.Assert(Leafs.Count == 0);
        Debug.Assert(DefaultLeaf.Length == 0);
        Debug.Assert(ActiveLatches == 0);
        Debug.Assert(!WasShared);
        Debug.Assert(!IsMutating);
        Debug.Assert(!Lease.HasValue);
    }
}