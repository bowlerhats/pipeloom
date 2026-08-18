using System;
using System.Collections.Generic;
using System.Linq;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Types;

public sealed class PlGenericMany : PlGenericType
{
    public override string Name => "Many<>";
    public override bool IsSupportingGenericConversions => true;

    public PlGenericMany(IPipeLoomEngine engine)
        : base(typeof(Many<>), engine)
    {
    }

    public override PlTypeDef Construct(Type concreteType, IReadOnlyList<PlTypeDef> arguments)
    {
        var arg = arguments.Single();
        if (arg.IsFloating)
            throw new PipeLoomException($"Inner type of Many<TInner = {arg.Name}> should not be floating");
        
        if (arg.NativeType == null!)
            throw new PipeLoomException($"Native inner type of Many<TInner = {arg.Name}> is null?!");
        
        return new PlManyOf(concreteType, this, arguments.Single(), this.Engine);
    }

    public override void BuildGenericConverter<TSourceInner, TTargetInner>(ConverterBuilderParams builderParams)
    {
        var innerConverter = builderParams.InnerConverter;

        var innerSourceType = this.Engine.TypeOf<TSourceInner>();
        
        builderParams.Convertible
            .FromValue<Many<TSourceInner>>()
            .ToValue<Many<TTargetInner>>()
            .Using((in v) =>
            {
                var newList = new List<TTargetInner>();
                foreach (var input in v)
                {
                    var vInput = Variant.From(input, innerSourceType);
                    var vOutput = innerConverter.Convert(in vInput);
                    var output = vOutput.Unpack<TTargetInner>();
                    newList.Add(output);
                }

                return new Many<TTargetInner>(newList);
            });
    }
}

public class PlMany : PlTypeDef
{
    public override string Name => "Many";
    public override PlTypeCardinality Cardinality => PlTypeCardinality.Many;
    public override bool IsFloating => true;
    public override Type NativeType => typeof(Many<>);
    
    public PlMany(IPipeLoomEngine engine) : base(engine)
    {
    }

    protected override Variant GetDefaultValue()
    {
        throw new NotImplementedException();
    }
}

public sealed class PlManyOf : PlMany, IPlConstructed<PlGenericMany>
{
    public override string Name { get; }
    public override bool IsFloating => false;
    public override Type NativeType { get; }

    public PlGenericMany GenericType { get; }
    public IReadOnlyList<PlTypeDef> GenericArguments { get; }
    
    public PlTypeDef InnerType { get; }
    
    public PlManyOf(
        Type concreteType,
        PlGenericMany genericType,
        PlTypeDef innerType,
        IPipeLoomEngine engine
        ) : base(engine)
    {
        this.NativeType = concreteType;
        
        this.GenericType = genericType;
        this.GenericArguments = [innerType];
        this.InnerType = innerType;

        this.Name = $"Many<{innerType.Name}>";
    }
}

public abstract class PlMany<T> : PlMany
{
    public override string Name => field ??= $"Scalar<{typeof(T).Name}>";
    public override bool IsFloating => false;
    public override Type NativeType => typeof(T);
    
    protected PlMany(IPipeLoomEngine engine) : base(engine)
    {
    }
}