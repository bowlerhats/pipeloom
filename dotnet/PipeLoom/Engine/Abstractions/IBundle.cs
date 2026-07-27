namespace PipeLoom.Engine.Abstractions;

public interface IReadOnlyBundle
{
    IReadOnlyBundle<TAlter> As<TAlter>();
}

public interface IReadOnlyBundle<T> : IReadOnlyBundle
{
    //IReadOnlyBundle<TAlter> As<TAlter>();
}

public interface IBundle
{
    IBundle<TAlter> As<TAlter>();
}

public interface IBundle<T> : IBundle
{
    // [int level]: PartitionList -> IList<Partition<Variant>>
    // [Partition]: Many<T>
    
    // IEnumerable<Many<T>> Leafs
}
