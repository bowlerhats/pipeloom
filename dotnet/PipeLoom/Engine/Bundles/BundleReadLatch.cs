using System;
using System.Threading;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine.Bundles;

internal interface ILatching<T>
{
    BundleReadLatch<T> Latch();
    void LatchRelease(BundleState<T> state);
}

internal struct BundleReadLatch<T> : IDisposable
{
    public BundleState<T> State => _state ?? throw new PipeLoomException("Latch is gone");

    private ILatching<T>? _latching;
    private BundleState<T>? _state;
    private bool _disposed;
    
    public BundleReadLatch(BundleState<T> state, ILatching<T> latching)
    {
        _state = state;
        _latching = latching;
    }
    
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true))
            return;
        
        if (_state is not null)
            _latching?.LatchRelease(_state);
        
        _latching = null;
        _state = null;
    }
}