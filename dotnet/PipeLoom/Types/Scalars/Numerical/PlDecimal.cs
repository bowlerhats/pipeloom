using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Scalars.Numerical;

public class PlDecimal : PlNumber<decimal>
{
    public PlDecimal(IPipeLoomEngine engine) : base(engine)
    {
    }
}