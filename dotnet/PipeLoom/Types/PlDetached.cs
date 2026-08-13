using System;
using System.Collections.Generic;
using System.Linq;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types;

public sealed class PlGenericDetached : PlGenericType
{
    public override string Name => "Detached<>";
    
    public PlGenericDetached(IPipeLoomEngine engine)
        : base(typeof(Detached<>), engine)
    {
    }

    public override PlTypeDef Construct(Type concreteType, IReadOnlyList<PlTypeDef> arguments)
    {
        return new PlDetached(concreteType, this, arguments.Single(), this.Engine);
    }
}

public sealed class PlDetached : PlTypeDef, IPlConstructed<PlGenericDetached>, IPlCustomInputArgProvider
{
    public override string Name { get; }
    public override PlTypeCardinality Cardinality => PlTypeCardinality.Unknown;
    public override bool IsFloating => false;
    
    public override Type NativeType { get; }
    public PlGenericDetached GenericType { get; }
    public IReadOnlyList<PlTypeDef> GenericArguments { get; }
    
    public PlTypeDef InnerType { get; }
    
    public PlDetached(
        Type concreteType,
        PlGenericDetached genericType,
        PlTypeDef innerType,
        IPipeLoomEngine engine)
        : base(engine)
    {
        this.NativeType = concreteType;
        this.GenericType = genericType;
        this.GenericArguments = [ innerType ];
        this.InnerType = innerType;

        this.Name = $"Detached<{innerType.Name}>";
    }

    protected override Variant GetDefaultValue()
    {
        throw new NotSupportedException();
    }

    public bool TryProvide(IStepState state, int childIndex, out Variant providedInputArg)
    {
        providedInputArg = Variant.From(new Detached<Variant>((StepState)state, childIndex));
        return true;
    }
}
