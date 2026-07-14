namespace PipeLoom.Engine.Abstractions;

public interface IReadOnlyBundle<T>
{
    //IReadOnlyBundle<TAlter> As<TAlter>();
}

public interface IBundle<T>
{
    // [int level]: PartitionList -> IList<Partition<Variant>>
    // [Partition]: Many<T>
    
    // IEnumerable<Many<T>> Leafs
    
    //IBundle<TAlter> As<TAlter>();
}
