namespace PipeLoom.Engine.Abstractions.Bundles;

public readonly record struct BundlePartitionEntry<T>(
    PartitionPath Path,
    Many<T> Leaf
);
