using System.Threading.Tasks;

namespace PipeLoom.Engine.Abstractions.Bundles;

public interface IErasedBundleView
{
    IBundle Bundle { get; }
    
    Many<Variant> GetErasedLeaf(PartitionPath path);
    
    void SetLeaf(PartitionPath path, Many<Variant> leaf);
    void SetLeaf(PartitionPath path, Variant singularLeaf);
    
    Many<Variant> Flatten();
    
    void Add(Variant item);
    bool TryAdd(Variant item);
    ValueTask AddAsync(Variant item);
    ValueTask<bool> TryAddAsync(Variant item);
}
