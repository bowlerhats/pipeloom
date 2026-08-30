using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PipeLoom.Engine.Pools;

namespace PipeLoom.Engine.Abstractions.Bundles.ListSources;

internal sealed class SingleItemSource<T> : IListSource<T>, IPoolReturnable, IUnsafeSpanProvider<T>
{
    public IWeaveContext? Context => _context;
    
    public int Count => 1;

    public T Item
    {
        get => _container[0];
        set => _container[0] = value;
    }

    private WeaveContext? _context;
    
    private readonly T[] _container = [default!];

    public void Bind(WeaveContext context)
    {
        _context = context;
        
        this.Item = default!;
    }

    public void Unbind()
    {
        this.Item = default!;
        
        _context = null;
    }
    
    public T GetItem(int index)
    {
        return index switch
        {
            0 => this.Item,
            _ => throw new IndexOutOfRangeException()
        };
    }

    public ListSourceEnumerator<T> GetEnumerator()
    {
        return new ListSourceEnumerator<T>(this);
    }

    public IEnumerable<T> AsEnumerable()
    {
        yield return this.Item;
    }

    public List<T> ToList()
    {
        return [this.Item];
    }

    public bool TryAddImmutable(T item, [MaybeNullWhen(false)] out IListSource<T> newSource)
    {
        newSource = null;
        
        return false;
    }

    ReturnResult IPoolReturnable.OnReturn(IObjectPool pool)
    {
        this.Unbind();
        
        return ReturnResult.Ok();
    }

    public ReadOnlySpan<T> UnsafeAsSpan()
    {
        return _container.AsSpan();
    }

    public ReadOnlyMemory<T> UnsafeAsMemory()
    {
        return _container.AsMemory();
    }
}