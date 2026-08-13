using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Scalars.Numerical;

public class PlUint : PlWholeNumber<uint>
{
    public PlUint(IPipeLoomEngine engine) : base(engine)
    {
    }
}