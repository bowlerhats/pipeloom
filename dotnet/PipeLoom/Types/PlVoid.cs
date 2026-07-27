using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types;

public sealed class PlVoid : PlVariant
{
    public override string Name => "Void";
    
    public PlVoid(IPipeLoomEngine engine) : base(engine)
    {
    }

    public override Variant ToVariant(Variant _)
    {
        return this.DefaultValue;
    }
}