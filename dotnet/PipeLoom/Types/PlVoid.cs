using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types;

public sealed class PlVoid : PlVariant
{
    public override string Name => "Void";
    public override bool IsFloating => true;
    
    public PlVoid(IPipeLoomEngine engine) : base(engine)
    {
    }

    protected override void SetupMyConverters(scoped in FromDefConverter fromMyself)
    {
        base.SetupMyConverters(in fromMyself);

        fromMyself.To<Variant>().Using(static (_, in _) => Variant.Undefined);
    }
}