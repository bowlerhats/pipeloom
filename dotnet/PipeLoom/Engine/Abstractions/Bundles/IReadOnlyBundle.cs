using System;

namespace PipeLoom.Engine.Abstractions.Bundles;

public interface IReadOnlyBundle
{
    IWeaveContext Context { get; }
    
    PlTypeDef ItemType { get; }
    PlTypeDef LeafType { get; }
    
    int PartitionCount { get; }
    
    // IErasedReadOnlyBundleView Erased { get; }

    Variant GetLeafAsPackedToVariant(PartitionPath path);
    
    ReadOnlyMemory<PartitionPath> Paths { get; }
    
    Variant PackAsVariant();
}

public interface IReadOnlyBundle<T> : IReadOnlyBundle
{
    // ReadOnlyMemory<BundlePartitionEntry<T>> Partitions { get; }
    
    // IReadOnlyList<BundlePartitionEntry<T>> Partitions { get; }

    // Many<T> Flatten();

    IBundle<T> Mutate();
}