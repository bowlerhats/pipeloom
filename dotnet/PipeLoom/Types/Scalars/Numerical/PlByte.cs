using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Scalars.Numerical;

public class PlByte : PlWholeNumber<byte>
{
    public PlByte(IPipeLoomEngine engine) : base(engine)
    {
    }
}