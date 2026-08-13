using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Scalars;

public class PlLong : PlWholeNumber<long>
{
    public PlLong(IPipeLoomEngine engine) : base(engine)
    {
    }
}