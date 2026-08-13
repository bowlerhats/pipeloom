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

        registrator.AsUnary<Variant>().Function(IsNotNull);
    }
    
    public static bool IsNotNull(Variant v)
    {
        if (v.IsUndefined)
            return false;
        
        return !v.IsPureReference || v.Reference is null;
    }
}