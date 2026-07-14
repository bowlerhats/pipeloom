using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;

namespace PipeLoom.Operators;

public class PlOpIsNull : PlOperatorClass
{
    public PlOpIsNull(IPipeLoomEngine engine) : base(engine, "isNull")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.Unary(IsNull);
    }
    
    public static Variant IsNull(scoped in Variant v)
    {
        var isnull = !v.IsDefined || v is { IsReference: true, Reference: null };
        return Variant.From(isnull);
    }
}