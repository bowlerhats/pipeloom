using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PipeLoom.Engine.Abstractions.Bundles.ListSources;

public interface IListSource
{
    IWeaveContext? Context { get; }
}

public interface IListSource<T> : IListSource
{
    public int Count { get; }

    T GetItem(int index);

    ListSourceEnumerator<T> GetEnumerator();
    
    IEnumerable<T> AsEnumerable();
    
    List<T> ToList();

    bool TryAddImmutable(T item, [MaybeNullWhen(false)] out IListSource<T> newSource);
}

public struct ListSourceEnumerator<T> : IEnumerator<T>
{
    public T Current { get; private set; }
    object? IEnumerator.Current => this.Current;
    
    private readonly IListSource<T> _source;
    private readonly int _end;
    private int _pos = -1;
    
    public ListSourceEnumerator(IListSource<T> source)
    {
        _source = source;
        
        _end = source.Count;
        
        this.Current = default!;
    }

    public void Dispose()
    {
        this.Reset();
    }

    public bool MoveNext()
    {
        if (++_pos < _end)
        {
            this.Current = _source.GetItem(_pos);
            return true;
        }

        this.Current = default!;

        return false;
    }

    public void Reset()
    {
        _pos = -1;
        this.Current = default!;
    }
}