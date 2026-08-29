using System.Collections;
using System.Collections.Generic;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Bundles;

namespace PipeLoom.Engine.Bundles;

internal sealed class BundlePartitions<T> : IBundlePartitions<T>
{
    public IBundle<T> Bundle => _bundle;

    private Bundle<T> _bundle;
    
    internal BundlePartitions(Bundle<T> bundle)
    {
        _bundle = bundle;
    }

    public BundlePartitionEnumerator<T> GetEnumerator()
    {
        var latch = _bundle.Latch();
        return new BundlePartitionEnumerator<T>(latch);
    }
}

public struct BundlePartitionEnumerator<T> : IEnumerator<BundlePartitionEntry<T>>
{
    public BundlePartitionEntry<T> Current { get; private set; }
    object IEnumerator.Current => this.Current;

    private bool _skipLatchDispose;
    private BundleReadLatch<T> _latch;
    private Dictionary<PartitionPath, Many<T>>.Enumerator _leafEnumerator;
    private bool _hasPartitions;
    private bool _ended = false;
    
    internal BundlePartitionEnumerator(BundleReadLatch<T> latch, bool skipLatchDispose = false)
    {
        _skipLatchDispose = skipLatchDispose;
        _latch = latch;
        
        _hasPartitions = latch.State.HasPartitions;
        _leafEnumerator = _hasPartitions
            ?latch.State.Leafs.GetEnumerator()
            : default;
        
        this.Current = _hasPartitions
            ? default!
            : new BundlePartitionEntry<T>(PartitionPath.Default, latch.State.DefaultLeaf);
    }
    
    public void Dispose()
    {
        if (_hasPartitions)
            _leafEnumerator.Dispose();
        
        if (!_skipLatchDispose)
            _latch.Dispose();
        
        this.Current = default!;
    }

    public bool MoveNext()
    {
        if (!_hasPartitions)
        {
            if (_ended)
                return false;
            
            _ended = true;
            return _ended;
        }
        
        if (!_leafEnumerator.MoveNext())
        {
            this.Current = default!;
            return false;
        }

        this.Current = new BundlePartitionEntry<T>(_leafEnumerator.Current.Key, _leafEnumerator.Current.Value);
        
        return true;
    }

    public void Reset()
    {
        if (_hasPartitions)
        {
            _leafEnumerator.Dispose();
            _leafEnumerator = _latch.State.Leafs.GetEnumerator();
            this.Current = default!;
        }
        else
        {
            _ended = false;
        }
    }
}