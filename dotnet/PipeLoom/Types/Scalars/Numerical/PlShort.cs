using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Scalars.Numerical;

public class PlShort : PlWholeNumber<short>
{
    public PlShort(IPipeLoomEngine engine) : base(engine)
    {
    }
}