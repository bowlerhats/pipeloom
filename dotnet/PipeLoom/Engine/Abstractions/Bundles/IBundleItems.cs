using PipeLoom.Engine.Bundles;

namespace PipeLoom.Engine.Abstractions.Bundles;

public interface IBundleItems<T>
{
    public int Count { get; }
    
    BundleItemEnumerator<T> GetEnumerator();
}