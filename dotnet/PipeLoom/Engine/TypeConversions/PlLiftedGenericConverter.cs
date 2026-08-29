using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Engine.TypeConversions;

internal sealed class PlLiftedGenericConverter: PlConverter
{
    public required PlConverter VDirect { get; init; }
    
    public required IPlConverter? HSourceConverter { get; init; }
    public required IPlConverter? HTargetConverter { get; init; }
    
    public PlLiftedGenericConverter(
        IPlConstructed gSource,
        IPlConstructed gTarget,
        IPipeLoomEngine engine
        ) : base(gSource.SelfType, gTarget.SelfType, engine)
    {
    }

    public override Variant Convert(IWeaveContext context, scoped in Variant value)
    {
        var left = value;
        if (this.HSourceConverter is not null)
        {
            left = this.HSourceConverter.Convert(context, left);
        }

        var right = this.VDirect.Convert(context, left);

        if (this.HTargetConverter is not null)
        {
            right = this.HTargetConverter.Convert(context, right);
        }

        return right;
    }
}