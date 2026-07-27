using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Scalars;

public class PlText : PlScalar<string>
{
    public override string Name => "Text";
    
    public PlText(IPipeLoomEngine engine) : base(engine)
    {
    }
}