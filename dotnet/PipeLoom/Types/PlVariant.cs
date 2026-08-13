using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types;

public class PlVariant : PlTypeDef<Variant>
{
    public override string Name => "Variant";
    public override PlTypeCardinality Cardinality => PlTypeCardinality.One;
    public override bool IsFloating => false;

    public PlVariant(IPipeLoomEngine engine)
        : base(engine)
    {
        
    }

    protected override Variant GetDefaultValue()
    {
        return Variant.From(Variant.Undefined, this);
    }
}