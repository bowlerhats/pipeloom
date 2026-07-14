using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;

namespace PipeLoom.Operators;

public class PlOpIsNotNull : PlOperatorClass
{
    public PlOpIsNotNull(IPipeLoomEngine engine) : base(engine, "isNotNull")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.Unary(IsNotNull);
    }
    
    public static Variant IsNotNull(scoped in Variant v)
    {
        if (!v.IsDefined)
            return Variant.From(false);
        
        return v.IsReference
            ? Variant.From(v.Reference is null)
            : Variant.From(true);
    }
}