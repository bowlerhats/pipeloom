using System.Collections.Generic;

namespace PipeLoom.Engine.Abstractions;

public interface IBundlePartition
{
    public int Length { get; }
    
    Many<Variant> Leaf { get; }

    Variant this[int index] { get; set; }
}

public interface IReadOnlyPartition
{
    
}

public interface IReadOnlyBundle
{
    IReadOnlyList<IBundlePartition> Partitions { get; }
    
    // Many<Variant> this[IBundlePartition partition] { get; }
    
    
    // IReadOnlyBundle<TAlter> As<TAlter>();

    // IBundle<T> Copy<T>();

    Variant ToVariant();
}

public interface IReadOnlyBundle<T> : IReadOnlyBundle
{
    //IReadOnlyBundle<TAlter> As<TAlter>();
}

public interface IBundle : IReadOnlyBundle
{
    void SetMany(IBundlePartition partition, Many<Variant> leaf);
    void SetSingle(IBundlePartition partition, Variant leaf);
    
    // IBundle<TAlter> As<TAlter>();
}

public interface IBundle<T> : IBundle, IReadOnlyBundle<T>
{
    // [int level]: PartitionList -> IList<Partition<Variant>>
    // [Partition]: Many<T>
    
    // IEnumerable<Many<T>> Leafs
    
    //void AddPartition()
}
