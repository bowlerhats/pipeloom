using PipeLoom.Engine;

namespace PipeLoom.Types;

public sealed class PlNever : PlTypeDef
{
    public override string Name => "never";
    public override PlTypeCardinality Cardinality => PlTypeCardinality.Unknown;

    public PlNever(IPipeLoomEngine engine)
        : base(engine)
    {
    }
}