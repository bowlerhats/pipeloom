namespace PipeLoom.Engine.Abstractions.Bundles;

public interface IErasedReadOnlyBundleView
{
    IReadOnlyBundle Bundle { get; }
    
    Many<Variant> GetErasedLeaf(PartitionPath path);
    
    Many<Variant> Flatten();
}