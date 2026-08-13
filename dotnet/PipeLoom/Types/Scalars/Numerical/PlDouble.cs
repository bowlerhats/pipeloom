using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Scalars.Numerical;

public class PlDouble : PlNumber<double>
{
    public PlDouble(IPipeLoomEngine engine) : base(engine)
    {
    }
}