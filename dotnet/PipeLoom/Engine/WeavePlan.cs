using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine;

public class WeavePlan : IDisposable
{
    public IPipeLoomEngine Engine { get; }
    public WeaveNode RootNode { get; }

    public IReadOnlyCollection<WeaveNode> Nodes => _nodes;

    public bool IsFused => _nodes.Count > 0 && _nodes.All(d => d.IsFused);

    public PlTypeDef OutputType => this.RootNode.ReturnType;
    
    private readonly HashSet<WeaveNode> _nodes = [];
    private readonly SemaphoreSlim _fuseLock = new(1);
    private bool _disposed;
    
    public WeavePlan(IPipeLoomEngine engine)
    {
        this.Engine = engine;
        this.RootNode = new WeaveNode(this, "sequence");
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fuseLock.Dispose();
        }
    }

    protected void CheckDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true))
            return;
        
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask Fuse<TOutput>()
    {
        this.CheckDisposed();
        
        if (!await _fuseLock.WaitAsync(MagicNumbers.FuseLockWaitMs))
            throw new PipeLoomException("Fuse lock timed out");
        
        try
        {
            foreach (var weaveNode in this.Nodes)
            {
                weaveNode.ResetFuse(true);
            }

            this.RootNode.RequiredReturnType = this.Engine.TypeOf<TOutput>();
            await this.RootNode.Fuse();
        }
        finally
        {
            _fuseLock.Release();
        }
    }

    public WeavePlan WithInputCarry<T>()
    {
        this.RootNode.CarryType = this.Engine.TypeOf<T>();
        return this;
    }

    internal void AddNode(WeaveNode node)
    {
        this.CheckDisposed();
        
        _nodes.Add(node);
    }
}