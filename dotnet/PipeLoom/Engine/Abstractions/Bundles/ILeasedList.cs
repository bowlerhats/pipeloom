using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PipeLoom.Engine.Abstractions.Bundles;


public interface IUnsafeSpanProvider<T>
{
    ReadOnlySpan<T> UnsafeAsSpan();
    ReadOnlyMemory<T> UnsafeAsMemory();
}

public interface IReadOnlyLeasedList<T> : IReadOnlyList<T>
{
    public long Version { get; }
    
    public new T this[int index] { get; }

    new LeasedListEnumerator<T> GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return this.GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    ILeasedList<T> Clone();
}

public interface ILeasedList<T> : IList<T>, IReadOnlyLeasedList<T>
{
    public new int Count { get; }
    
    /// <summary>
    /// Unsafe index-based access. Don't use it concurrently. <br/>
    /// It does not lock in favor of fast access.
    /// </summary>
    /// <param name="index"></param>
    public new T this[int index] { get; set; }
    
    void ReplaceItems(ReadOnlySpan<T> items);
}

public struct LeasedListEnumerator<T> : IEnumerator<T>
{
    public T Current => _current;
    object? IEnumerator.Current => this.Current;

    private IReadOnlyLeasedList<T> _list;
    private readonly long _version;
    private readonly int _count;
    private T[] _items;
    private bool _broken;
    private int _pos;
    private T _current;
    
    public LeasedListEnumerator(IReadOnlyLeasedList<T> list, long version, T[] items)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(items);
        
        _list = list;
        _version = version;
        _items = items;
        _count = list.Count;
        
        _current = default!;
        _pos = -1;
    }

    public void Dispose()
    {
        _broken = true;
        this.Invalidate();
    }

    public bool MoveNext()
    {
        if (_broken)
            throw new InvalidOperationException("ILeasedList changed since start of enumeration");

        if (++_pos >= _count)
        {
            _current = default!;
            return false;
        }
        
        _current = _items[_pos];

        if (_list.Version == _version)
            return true;
        
        _current = default!;
        
        this.Break();
        
        throw new UnreachableException();
    }

    public void Reset()
    {
        this.ThrowIfBroken();

        _pos = -1;
        _current = default!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfBroken()
    {
        if (_broken || _items is null || _list is null)
            throw new InvalidOperationException("ILeasedList changed since start of enumeration");
    }

    private void Break()
    {
        _broken = true;
        this.Invalidate();
        this.ThrowIfBroken();
    }
    
    private void Invalidate()
    {
        _current = default!;
        _items = null!;
        _list = null!;
        _pos = -1;
    }
}