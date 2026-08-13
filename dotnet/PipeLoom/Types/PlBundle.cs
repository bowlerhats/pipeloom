using System;
using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types;

public class PlBundle : PlTypeDef
{
    public override string Name => "Bundle";
    public override PlTypeCardinality Cardinality => PlTypeCardinality.Many;
    public override bool IsFloating => false;
    
    public PlBundle(IPipeLoomEngine engine) : base(engine)
    {
    }

    public override Type NativeType { get; }
    protected override Variant GetDefaultValue()
    {
        throw new NotImplementedException();
    }
}