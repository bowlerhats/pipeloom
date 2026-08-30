using System;

namespace PipeLoom.Engine.Abstractions.Bundles;

public interface IErasedReadOnlyBundleView
{
    IReadOnlyBundle Bundle { get; }
    
    // ReadOnlyMemory<BundlePartitionEntry<Variant>> Partitions { get; }
    
    // Many<Variant> GetErasedLeaf(PartitionPath path);
    
    Many<Variant> Flatten();
}