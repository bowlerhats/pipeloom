using System;
using System.Collections.Generic;
using System.Linq;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Types.Scalars;

namespace PipeLoom.Types;

public sealed class PlGenericScalar : PlGenericType
{
    public override string Name => "Scalar<>";
    
    public PlGenericScalar(IPipeLoomEngine engine)
        : base(typeof(Scalar<>), engine)
    {
    }

    public override PlTypeDef Construct(Type concreteType, IReadOnlyList<PlTypeDef> arguments)
    {
        var arg = arguments.Single();
        if (arg.IsFloating)
            throw new PipeLoomException($"Inner type of Scalar<TInner = {arg.Name}> should not be floating");
        
        if (arg.NativeType == null!)
            throw new PipeLoomException($"Native inner type of Scalar<TInner = {arg.Name}> is null?!");
        
        if (!arg.NativeType.IsValueType)
            throw new PipeLoomException($"Native inner type of Scalar<TInner = {arg.Name}> for constructed typedefs should be a value type");
        
        return new PlScalarOf(concreteType, this, arguments.Single(), this.Engine);
    }
}

public class PlScalar : PlTypeDef
{
    public override string Name => "Scalar";
    public override PlTypeCardinality Cardinality => PlTypeCardinality.One;
    public override bool IsFloating => true;
    public override Type NativeType => typeof(Scalar<>);

    public PlScalar(IPipeLoomEngine engine)
        : base(engine)
    {
        
    }

    protected override Variant GetDefaultValue()
    {
        return Variant.Undefined;
    }
}

public sealed class PlScalarOf : PlScalar, IPlConstructed<PlGenericScalar>
{
    public override string Name { get; }
    public override bool IsFloating => false;
    public override Type NativeType { get; }

    public PlGenericScalar GenericType { get; }
    public IReadOnlyList<PlTypeDef> GenericArguments { get; }
    
    public PlTypeDef InnerType { get; }
    
    public PlScalarOf(
        Type concreteType,
        PlGenericScalar genericType,
        PlTypeDef innerType,
        IPipeLoomEngine engine
        ) : base(engine)
    {
        this.NativeType = concreteType;
        
        this.GenericType = genericType;
        this.GenericArguments = [innerType];
        this.InnerType = innerType;

        this.Name = $"Scalar<{innerType.Name}>";
    }
}

public abstract class PlScalar<TScalar> : PlScalar
{
    public override string Name => field ??= $"Scalar<{typeof(TScalar).Name}>";
    public override bool IsFloating => false;
    public override Type NativeType => typeof(TScalar);

    protected PlScalar(IPipeLoomEngine engine)
        : base(engine)
    {
    }
}