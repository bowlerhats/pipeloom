using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Scalars;

public class PlBool : PlScalar<bool>
{
    public PlBool(IPipeLoomEngine engine) : base(engine)
    {
    }
}