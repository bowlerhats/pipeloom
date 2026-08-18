using System;
using System.Collections.Generic;
using System.Linq;

namespace PipeLoom.Engine.Abstractions;

interface IGenericConvertible
{
    bool TryConvertTo<U>(in Variant v, out Variant converted);
}

public readonly struct Many<T> : IVariantDecomposable<Many<T>>, IForcedStaticalyInitialized
{
    private readonly List<T>? _items;
    
    private static readonly List<T> Empty = [];
    
    public int Length => _items?.Count ?? 0;
    public T this[int index] => _items is not null ? _items[index] : throw new IndexOutOfRangeException();

    public Many(List<T> items)
    {
        _items = items;
    }
    
    public Many(IEnumerable<T> items)
    {
        _items = [.. items];
    }

    public IEnumerator<T> GetEnumerator()
    {
        return (_items ?? Empty).GetEnumerator();
    }
    
    public List<T> ToList()
    {
        return _items?.ToList() ?? [];
    }

    public IReadOnlyList<T> AsList()
    {
        return _items ?? Empty;
    }
    
    #region Decomposable

    static Many()
    {
        VariantDecomposeRegistrar<Many<T>>.EnsureRegistered();
        DoubleDispatch<T>.Register();
    }
    
    public (object? reference, Many<T> bare) DecomposeForVariant()
    {
        return (_items, default);
    }

    public static Many<T> ComposeFromPair(object? reference, Many<T> bare)
    {
        return reference is null ? default : new Many<T>((List<T>)reference);
    }
    
    #endregion
}
