using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types;

public abstract class PlScalar<TScalar> : PlTypeDef<TScalar>
{
    public override PlTypeCardinality Cardinality => PlTypeCardinality.One;
    
    protected PlScalar(IPipeLoomEngine engine) : base(engine)
    {
    }
}

