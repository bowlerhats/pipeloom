using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Scalars.Numerical;

public class PlUshort : PlWholeNumber<ushort>
{
    public PlUshort(IPipeLoomEngine engine) : base(engine)
    {
    }
}