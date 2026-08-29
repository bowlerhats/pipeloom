using System;
using System.Collections.Generic;
using System.Linq;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Bundles;

namespace PipeLoom.Types;

public class PlGenericBundle : PlGenericType
{
    public override string Name => "IBundle<>";
    public override bool SupportsHomomorphicConversion => true;

    public PlGenericBundle(IPipeLoomEngine engine)
        : base(typeof(IBundle<>), engine)
    {
    }

    protected override PlTypeDef Construct(Type concreteType, IReadOnlyList<PlTypeDef> arguments)
    {
        return new PlBundleOf(concreteType, this, arguments.Single(), this.Engine);
    }

    public override IPlConverter BuildHomomorphicConverter<TSourceInner, TTargetInner>(ConverterBuilderParams builderParams)
    {
        var innerSourceType = this.Engine.TypeOf<TSourceInner>();
        
        if (typeof(TTargetInner) == typeof(Variant))
        {
            return builderParams.Convertible
                .FromRef<IBundle<TSourceInner>>()
                .ToRef<IBundle<Variant>>()
                .Using((_, v) => v.ConvertTo(innerSourceType, static (sType, d) => Variant.From(d, sType)));
        }
        
        var innerConverter = builderParams.InnerConverter;

        return builderParams.Convertible
            .FromRef<IBundle<TSourceInner>>()
            .ToRef<IBundle<TTargetInner>>()
            .Using((context, v) =>
            {
                return v.ConvertTo((context, innerConverter, innerSourceType), Convert);

                static TTargetInner Convert((IWeaveContext context, IPlConverter innerConverter, PlTypeDef innerSourceType) state, TSourceInner source)
                {
                    var vSource = Variant.From(source, state.innerSourceType);
                    var vTarget = state.innerConverter.Convert(state.context, vSource);
                    return vTarget.Unpack<TTargetInner>();
                }
            });
    }
}

public class PlBundle : PlTypeDef
{
    public override string Name => "IBundle";
    public override PlTypeCardinality Cardinality => PlTypeCardinality.Many;
    public override bool IsFloating => false;
    public override Type NativeType => typeof(IBundle);
    
    public PlBundle(IPipeLoomEngine engine) : base(engine)
    {
    }

    protected override Variant GetDefaultValue()
    {
        return Variant.Undefined;
    }
}

public class PlBundleOf : PlBundle, IPlConstructed<PlGenericBundle>
{
    public override string Name { get; }
    public override bool IsFloating => false;
    public override Type NativeType { get; }

    public PlGenericBundle GenericType { get; }
    public IReadOnlyList<PlTypeDef> GenericArguments { get; }
    
    public PlTypeDef InnerType { get; }

    PlTypeDef IPlConstructed.SelfType => this;
    
    public PlBundleOf(
        Type concreteType,
        PlGenericBundle genericType,
        PlTypeDef innerType,
        IPipeLoomEngine engine
    ) : base(engine)
    {
        this.NativeType = concreteType;
        
        this.GenericType = genericType;
        this.GenericArguments = [innerType];
        this.InnerType = innerType;

        this.Name = $"IBundle<{innerType.Name}>";
    }
}