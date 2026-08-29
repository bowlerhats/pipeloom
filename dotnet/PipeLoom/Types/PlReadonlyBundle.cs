using System;
using System.Collections.Generic;
using System.Linq;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Bundles;

namespace PipeLoom.Types;

public class PlGenericReadOnlyBundle : PlGenericType
{
    public override string Name => "IReadOnlyBundle<>";
    
    public PlGenericReadOnlyBundle(IPipeLoomEngine engine)
        : base(typeof(IReadOnlyBundle<>), engine)
    {
    }
    
    protected override PlTypeDef Construct(Type concreteType, IReadOnlyList<PlTypeDef> arguments)
    {
        return new PlReadOnlyBundleOf(concreteType, this, arguments.Single(), this.Engine);
    }
}

public class PlReadonlyBundle : PlTypeDef
{
    public override string Name => "IReadOnlyBundle";
    public override PlTypeCardinality Cardinality => PlTypeCardinality.One;
    public override bool IsFloating => true;
    public override Type NativeType => typeof(IReadOnlyBundle);
    
    public PlReadonlyBundle(IPipeLoomEngine engine) : base(engine)
    {
    }
    
    protected override Variant GetDefaultValue()
    {
        throw new NotImplementedException();
    }
}


public sealed class PlReadOnlyBundleOf : PlScalar, IPlConstructed<PlGenericReadOnlyBundle>
{
    public override string Name { get; }
    public override bool IsFloating => false;
    public override Type NativeType { get; }

    public PlGenericReadOnlyBundle GenericType { get; }
    public IReadOnlyList<PlTypeDef> GenericArguments { get; }
    
    public PlTypeDef InnerType { get; }
    PlTypeDef IPlConstructed.SelfType => this;
    
    public PlReadOnlyBundleOf(
        Type concreteType,
        PlGenericReadOnlyBundle genericType,
        PlTypeDef innerType,
        IPipeLoomEngine engine
    ) : base(engine)
    {
        this.NativeType = concreteType;
        
        this.GenericType = genericType;
        this.GenericArguments = [innerType];
        this.InnerType = innerType;

        this.Name = $"IReadOnlyBundle<{innerType.Name}>";
    }
}