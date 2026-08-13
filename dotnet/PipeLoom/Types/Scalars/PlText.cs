using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Scalars;

public class PlText : PlTypeDef<string>
{
    public override string Name => "Text";
    public override PlTypeCardinality Cardinality => PlTypeCardinality.One;

    public PlText(IPipeLoomEngine engine) : base(engine)
    {
    }
}