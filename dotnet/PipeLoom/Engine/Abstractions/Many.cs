using System;
using System.Collections.Generic;

namespace PipeLoom.Engine.Abstractions;

public interface IMany
{
    
}

public readonly struct Many<T>
{
    public static Many<T> Wrap(List<T> list)
    {
        throw new NotImplementedException();
    }
    
    public int Length => throw new NotImplementedException();
    public T this[int index] => throw new NotImplementedException();

    public List<T> ToList()
    {
        throw new NotImplementedException();
    }
    // PartitionKey Partition
    // public IReadOnlyList<T> Items => throw new NotImplementedException();

    // public Many<TAlter> As<TAlter>()
    // {
    //     throw new NotImplementedException();
    // }

    public Variant ToVariant()
    {
        throw new NotImplementedException();
    }
}
