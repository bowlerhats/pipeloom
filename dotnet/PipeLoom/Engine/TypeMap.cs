using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine;

internal sealed class TypeMap
{
    private readonly PipeLoomEngine _engine;
    
    private readonly Dictionary<Type, PlGenericType> _generics = [];
    private readonly ConcurrentDictionary<Type, PlTypeDef> _typeDefs = [];

    public TypeMap(PipeLoomEngine engine)
    {
        _engine = engine;
    }
    
    public PlTypeDef TypeOf<T>()
    {
        return this.TypeOf(typeof(T));
    }

    private PlTypeDef TypeOf(Type type)
    {
        if (_typeDefs.TryGetValue(type, out var def))
            return def;

        if (type.IsGenericType && this.TryConstructGeneric(type, out var generic))
            return generic;
        
        throw new PipeLoomException("Unknown type");
    }

    private bool TryConstructGeneric(Type target, [MaybeNullWhen(false)] out PlTypeDef constructed)
    {
        if (target.IsGenericTypeDefinition)
            throw new PipeLoomException("Cannot construct an open generic type");
        
        constructed = null;

        if (!_generics.TryGetValue(target.GetGenericTypeDefinition(), out var genericType))
            return false;

        var args = target.GetGenericArguments().Select(this.TypeOf).ToList();
        constructed = genericType.Construct(target, args);
        
        if (constructed is null)
            throw new PipeLoomException("Failed to construct from open generic");

        _typeDefs.TryAdd(target, constructed);

        return true;
    }
}