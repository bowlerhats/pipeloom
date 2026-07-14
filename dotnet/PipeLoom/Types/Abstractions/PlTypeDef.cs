using PipeLoom.Engine;

namespace PipeLoom.Types.Abstractions;

public enum PlTypeCardinality
{ 
    Unknown = 0, 
    One = 1,
    Many = 2
}

public abstract class PlTypeDef
{
    public IPipeLoomEngine Engine { get; }
    public abstract string Name { get; }
    public abstract PlTypeCardinality Cardinality { get; }

    protected PlTypeDef(IPipeLoomEngine engine)
    {
        this.Engine = engine;
    }
}

public abstract class PlTypeDef<TNative> : PlTypeDef
{
    protected PlTypeDef(IPipeLoomEngine engine)
        : base(engine)
    {
    }
}