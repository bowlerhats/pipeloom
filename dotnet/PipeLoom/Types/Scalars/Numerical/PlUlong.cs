using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Scalars.Numerical;

public class PlUlong : PlWholeNumber<ulong>
{
    public PlUlong(IPipeLoomEngine engine) : base(engine)
    {
    }
}