using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types;

public sealed class PlVoid : PlVariant
{
    public override string Name => "Void";
    public override bool IsFloating => true;
    
    public PlVoid(IPipeLoomEngine engine) : base(engine)
    {
    }

    protected override void SetupConverters(scoped in FromDefConverter fromMyself)
    {
        base.SetupConverters(in fromMyself);

        fromMyself.To<Variant>().Using((in _) => Variant.Undefined);
    }
}