using System;
using System.Collections.Generic;
using System.Linq;
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

public sealed class PlDetached : PlTypeDef, IPlConstructed<PlGenericDetached>
{
    public override string Name { get; }
    public override PlTypeCardinality Cardinality => PlTypeCardinality.Unknown;
    
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

    public override bool IsAssignableTo(PlTypeDef other)
    {
        if (base.IsAssignableTo(other))
            return true;

        return this.InnerType == other;
    }

    public override Variant AssignTo(Variant value, PlTypeDef target)
    {
        
        return base.AssignTo(value, target);
    }

    protected override Variant GetDefaultValue()
    {
        throw new NotImplementedException();
    }
}
