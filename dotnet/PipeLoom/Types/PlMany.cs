using System;
using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types;

public class PlMany : PlTypeDef
{
    public override bool IsFloating => false;
    
    public PlMany(IPipeLoomEngine engine) : base(engine)
    {
    }

    public override string Name { get; }
    public override PlTypeCardinality Cardinality { get; }
    public override Type NativeType { get; }
    protected override Variant GetDefaultValue()
    {
        throw new NotImplementedException();
    }
}