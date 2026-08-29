using System;
using System.Collections.Generic;
using System.Linq;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Bundles;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Types;

public sealed class PlGenericMany : PlGenericType
{
    public override string Name => "Many<>";
    public override bool SupportsHomomorphicConversion => true;

    public PlGenericMany(IPipeLoomEngine engine)
        : base(typeof(Many<>), engine)
    {
    }

    protected override PlTypeDef Construct(Type concreteType, IReadOnlyList<PlTypeDef> arguments)
    {
        var arg = arguments.Single();
        if (arg.IsFloating)
            throw new PipeLoomException($"Inner type of Many<TInner = {arg.Name}> should not be floating");
        
        if (arg.NativeType == null!)
            throw new PipeLoomException($"Native inner type of Many<TInner = {arg.Name}> is null?!");
        
        return new PlManyOf(concreteType, this, arguments.Single(), this.Engine);
    }

    public override IPlConverter BuildHomomorphicConverter<TSourceInner, TTargetInner>(ConverterBuilderParams builderParams)
    {
        if (typeof(TTargetInner) == typeof(Variant))
        {
            return builderParams.Convertible
                .FromValue<Many<TSourceInner>>()
                .ToValue<Many<Variant>>()
                .Using(static (context, in v) => v.ToVariantMany(context));
        }
        
        var innerConverter = builderParams.InnerConverter;

        var innerSourceType = this.Engine.TypeOf<TSourceInner>();
        
        return builderParams.Convertible
            .FromValue<Many<TSourceInner>>()
            .ToValue<Many<TTargetInner>>()
            .Using((context, in v) =>
            {
                return v.ConvertTo(context, (context, innerConverter, innerSourceType), Convert);

                static TTargetInner Convert((IWeaveContext context, IPlConverter converter, PlTypeDef innerSourceType) state, TSourceInner input)
                {
                    var vInput = Variant.From(input, state.innerSourceType);
                    var vOutput = state.converter.Convert(state.context, in vInput);
                    return vOutput.Unpack<TTargetInner>();
                }
            });
    }

    protected internal override void SetupConverters(scoped in ConverterRegistrator convertible)
    {
        base.SetupConverters(in convertible);

        convertible
            .FromValue<Many<Variant>>()
            .ToRef<IBundle<Variant>>()
            .Using(static (context, in many) =>
            {
                var res = context.Bundles.Create<Variant>();
                res.SetLeaf(PartitionPath.Default, many);
                
                return res;
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
    PlTypeDef IPlConstructed.SelfType => this;
    
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