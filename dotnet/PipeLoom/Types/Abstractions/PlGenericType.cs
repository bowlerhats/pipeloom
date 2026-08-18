using System;
using System.Collections.Generic;
using System.Linq;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Types.Abstractions;

public abstract class PlGenericType : PlTypeDef, IDoubleDispatchCallback
{
    public sealed override Type NativeType { get; }
    public sealed override PlTypeCardinality Cardinality => PlTypeCardinality.Unknown;
    public override bool IsFloating => true;
    
    public virtual bool IsSupportingGenericConversions => false;

    protected PlGenericType(Type nativeOpenGeneric, IPipeLoomEngine engine)
        : base(engine)
    {
        this.NativeType = nativeOpenGeneric;
        
        if (!nativeOpenGeneric.IsGenericTypeDefinition)
        {
            throw new PipeLoomException("Generic type definitions expect open generic native types");
        }
    }

    public abstract PlTypeDef Construct(Type concreteType, IReadOnlyList<PlTypeDef> arguments);

    public void MakeGenericConverter(PlTypeDef source, PlTypeDef target, IPlConverter innerConverter, ConverterRegistrator convertible)
    {
        if (source is not IPlConstructed cSource || target is not IPlConstructed cTarget)
            return;

        var sArg = cSource.GenericArguments.Single();
        var tArg = cTarget.GenericArguments.Single();

        var p = new ConverterBuilderParams
        {
            InnerConverter = innerConverter,
            Convertible = convertible
        };
        
        IDoubleDispatched.Dispatch(sArg.NativeType, tArg.NativeType, this, p);
    }

    void IDoubleDispatchCallback.Dispatch<T, U>(object? state)
    {
        ArgumentNullException.ThrowIfNull(state);
        
        this.BuildGenericConverter<T, U>((ConverterBuilderParams)state);
    }

    public virtual void BuildGenericConverter<TSourceInner, TTargetInner>(ConverterBuilderParams builderParams)
    {
        throw new NotSupportedException($"{this.Name} does not support generic conversions");
    }

    protected sealed override Variant GetDefaultValue()
    {
        throw new NotSupportedException("Generic type defs don't have default values");
    }

    public sealed class ConverterBuilderParams
    {
        public required IPlConverter InnerConverter { get; init; }
        public required ConverterRegistrator Convertible { get; init; }
    }
}
