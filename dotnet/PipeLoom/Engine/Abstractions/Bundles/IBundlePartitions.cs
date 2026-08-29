using PipeLoom.Engine.Bundles;

namespace PipeLoom.Engine.Abstractions.Bundles;

public interface IBundlePartitions<T>
{
    IBundle<T> Bundle { get; }
    
    BundlePartitionEnumerator<T> GetEnumerator();
}