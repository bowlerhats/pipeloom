using System;
using System.Collections.Generic;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Types.Abstractions;

public abstract class PlGenericType : PlTypeDef
{
    public sealed override Type NativeType { get; }
    public sealed override PlTypeCardinality Cardinality => PlTypeCardinality.Unknown;

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

    protected sealed override Variant GetDefaultValue()
    {
        throw new NotSupportedException("Generic type defs don't have default values");
    }
}
