using System.Collections;
using System.Collections.Generic;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Bundles;

namespace PipeLoom.Engine.Bundles;

internal sealed class BundleItems<T> : IBundleItems<T>
{
    public IBundle<T> Bundle => _bundle;

    public int Count => _bundle.ItemCount;

    private Bundle<T> _bundle;

    public BundleItems(Bundle<T> bundle)
    {
        _bundle = bundle;
    }

    public BundleItemEnumerator<T> GetEnumerator()
    {
        var latch = _bundle.Latch();
        return new BundleItemEnumerator<T>(latch);
    }
}

public struct BundleItemEnumerator<T> : IEnumerator<T>
{
    public T Current { get; private set; }
    object? IEnumerator.Current => this.Current;

    private BundleReadLatch<T> _latch;
    private BundlePartitionEnumerator<T> _partitionEnumerator;
    private Many<T>.Enumerator _manyEnumerator;
    private bool _hasActiveManyEnumerator;
    
    internal BundleItemEnumerator(BundleReadLatch<T> latch)
    {
        _latch = latch;
        _partitionEnumerator = new BundlePartitionEnumerator<T>(latch, true);
        _hasActiveManyEnumerator = false;
        
        this.Current = default!;
    }

    public void Dispose()
    {
        if (_hasActiveManyEnumerator)
        {
            _manyEnumerator.Dispose();
            _hasActiveManyEnumerator = false;
        }

        _partitionEnumerator.Dispose();
        _latch.Dispose();
    }

    public bool MoveNext()
    {
        do
        {
            if (!_hasActiveManyEnumerator)
            {
                if (!_partitionEnumerator.MoveNext())
                    return false;

                // ReSharper disable once GenericEnumeratorNotDisposed
                _manyEnumerator = _partitionEnumerator.Current.Leaf.GetEnumerator();
                _hasActiveManyEnumerator = true;
            }

            if (!_manyEnumerator.MoveNext())
            {
                _manyEnumerator.Dispose();
                _manyEnumerator = default!;
                _hasActiveManyEnumerator = false;
                
                continue;
            }
            
            this.Current = _manyEnumerator.Current;
            
            return true;
        } while (true);
    }

    public void Reset()
    {
        if (_hasActiveManyEnumerator)
        {
            _manyEnumerator.Dispose();
            _manyEnumerator = default!;
            _hasActiveManyEnumerator = false;
        }
        
        _partitionEnumerator.Dispose();
        _partitionEnumerator = new BundlePartitionEnumerator<T>(_latch, true);
        
        this.Current = default!;
    }
}