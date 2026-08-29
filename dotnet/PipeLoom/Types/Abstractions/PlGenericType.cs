using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.TypeConversions;

namespace PipeLoom.Types.Abstractions;

public abstract class PlGenericType : PlTypeDef, IDoubleDispatchCallback<PlConverter>
{
    public sealed override Type NativeType { get; }
    public sealed override PlTypeCardinality Cardinality => PlTypeCardinality.Unknown;
    public override bool IsFloating => true;
    
    public virtual bool SupportsHomomorphicConversion => false;

    private readonly ConcurrentDictionary<PlTypeDef, IPlConstructed> _constructedSingleArg = [];

    protected PlGenericType(Type nativeOpenGeneric, IPipeLoomEngine engine)
        : base(engine)
    {
        this.NativeType = nativeOpenGeneric;
        
        if (!nativeOpenGeneric.IsGenericTypeDefinition)
        {
            throw new PipeLoomException("Generic type definitions expect open generic native types");
        }
    }

    protected abstract PlTypeDef Construct(Type concreteType, IReadOnlyList<PlTypeDef> arguments);

    internal PlTypeDef ConstructGeneric(Type concreteType, IReadOnlyList<PlTypeDef> arguments)
    {
        if (arguments.Count != 1)
            throw new NotSupportedException("Multiarg generics are not supported");

        if (_constructedSingleArg.TryGetValue(arguments.Single(), out var existing))
            return (PlTypeDef)existing;
        
        var def = this.Construct(concreteType, arguments);
        var gDef = (IPlConstructed)def;

        if (!_constructedSingleArg.TryAdd(gDef.GenericArguments[0], gDef))
            throw new PipeLoomException("Inconsistent generic construction");
        
        return def;
    }
    
    internal PlConverter MakeGenericConverter(PlTypeDef sourceArgType, PlTypeDef targetArgType, IPlConverter innerConverter, ConverterRegistrator convertible)
    {
        var p = new ConverterBuilderParams
        {
            InnerConverter = innerConverter,
            Convertible = convertible
        };
        
        return IDoubleDispatched.Dispatch(sourceArgType.NativeType, targetArgType.NativeType, this, p);
    }

    internal IPlConstructed? FindConstructedOfInner(PlTypeDef innerType)
    {
        return _constructedSingleArg.GetValueOrDefault(innerType);
    }
    
    internal IPlConstructed? FindOrCreateConstructedOfInner(PlTypeDef innerType)
    {
        var res = _constructedSingleArg.GetValueOrDefault(innerType);
        if (res is not null)
            return res;
        
        var constructable = Discovery.FindGeneric(this.NativeType, innerType.NativeType);
        if (constructable is not null)
        {
            res = (IPlConstructed)this.Engine.TypeMap.ConstructGeneric(constructable.NativeType, this);
        }

        return res;
    }

    PlConverter IDoubleDispatchCallback<PlConverter>.Dispatch<T, U>(object? state)
    {
        ArgumentNullException.ThrowIfNull(state);
        
        return (PlConverter)this.BuildHomomorphicConverter<T, U>((ConverterBuilderParams)state);
    }

    public virtual IPlConverter BuildHomomorphicConverter<TSourceInner, TTargetInner>(ConverterBuilderParams builderParams)
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
