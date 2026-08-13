using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Scalars;

public abstract class PlNumber<T> : PlScalar<T>
{
    protected PlNumber(IPipeLoomEngine engine) : base(engine)
    {
    }

    
}

public abstract class PlWholeNumber<T> : PlNumber<T>
{
    protected PlWholeNumber(IPipeLoomEngine engine) : base(engine)
    {
    }
}
