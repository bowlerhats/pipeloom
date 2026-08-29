using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Bundles;

namespace PipeLoom.Engine.Bundles;

internal sealed class DetachedPartitioner : IBundlePartitioner
{
    public bool IsAsync => true;

    private readonly Detached<Variant> _detached;

    public DetachedPartitioner(Detached<Variant> detached)
    {
        _detached = detached;
    }
    
    public Variant GetKey(scoped in Variant item, IWeaveContext context)
    {
        var vt = this.GetKeyAsync(item, context);
        
        return vt.IsCompletedSuccessfully
            ? vt.Result
            : vt.AsTask().GetAwaiter().GetResult();
    }

    public ValueTask<Variant> GetKeyAsync(Variant item, IWeaveContext context)
    {
        return context.StepDetached(_detached, item);
    }
}