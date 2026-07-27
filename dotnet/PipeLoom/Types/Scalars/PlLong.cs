using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Scalars;

public class PlLong : PlWholeNumber<long>
{
    public override string Name => "Int64";
    
    public PlLong(IPipeLoomEngine engine) : base(engine)
    {
    }
}