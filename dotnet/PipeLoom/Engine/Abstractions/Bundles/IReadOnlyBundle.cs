using System.Collections.Generic;

namespace PipeLoom.Engine.Abstractions.Bundles;

public interface IReadOnlyBundle
{
    IWeaveContext Context { get; }
    
    IErasedReadOnlyBundleView Erased { get; }
    
    IReadOnlyList<PartitionPath> Paths { get; }
    
    Variant PackAsVariant();
}

public interface IReadOnlyBundle<T> : IReadOnlyBundle
{
    IBundlePartitions<T> Partitions { get; }

    Many<T> Flatten();

    IBundle<T> Mutate();
}