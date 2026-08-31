using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;

namespace PipeLoom.Operators.Core;

public class PlOpIsNull : PlOperatorClass
{
    public PlOpIsNull(IPipeLoomEngine engine) : base(engine, "isNull")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<Variant>().Function(IsNull);
    }
    
    public static bool IsNull(Variant v)
    {
        var isnull = v.IsUndefined || v is { IsPureReference: true, Reference: null };
        
        return isnull;
    }
}