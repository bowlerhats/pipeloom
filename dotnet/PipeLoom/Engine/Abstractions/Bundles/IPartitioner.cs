using System.Threading.Tasks;

namespace PipeLoom.Engine.Abstractions.Bundles;

public interface IBundlePartitioner
{
    bool IsAsync { get; }
    
    Variant GetKey(scoped in Variant item, IWeaveContext context);

    ValueTask<Variant> GetKeyAsync(Variant item, IWeaveContext context);
}