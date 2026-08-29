using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine;

internal abstract class PlTypeInfo
{
    public Type NativeType { get; }
    
    public Type? OpenGeneric { get; protected set; }
    
    public Type? GenericArgument { get; protected set; }
    
    public bool IsGeneric { get; }
    public bool IsClosedGeneric { get; }

    protected PlTypeInfo(Type type)
    {
        this.NativeType = type;
        this.IsGeneric = type.IsGenericType;
        this.IsClosedGeneric = type.IsConstructedGenericType;
    }
}

internal sealed class PlTypeInfo<T>: PlTypeInfo
{
    public PlTypeInfo()
        : base(typeof(T))
    {
        if (typeof(T).IsConstructedGenericType)
        {
            this.OpenGeneric = typeof(T).GetGenericTypeDefinition();
            this.GenericArgument = typeof(T).GetGenericArguments()[0];
        }
    }
}

internal static class Discovery
{
    private static readonly ConcurrentDictionary<Type, PlTypeInfo> TypeInfo = [];
    
    public static void Discover<T>()
    {
        if (TypeInfo.ContainsKey(typeof(T)))
            return;

        var info = new PlTypeInfo<T>();
        TypeInfo.TryAdd(typeof(T), info);
    }

    public static PlTypeInfo GetTypeInfo<T>()
    {
        Discover<T>();
        return FindTypeInfo(typeof(T)) ?? throw new PipeLoomException($"Type '{typeof(T).FullName}' is not discovered");
    }

    public static PlTypeInfo? FindTypeInfo(Type type)
    {
        return TypeInfo.GetValueOrDefault(type);
    }

    public static IEnumerable<PlTypeInfo> GenericInstances(Type openGeneric)
    {
        var allTypes = TypeInfo.Values;
        foreach (var typeInfo in allTypes)
        {
            if (typeInfo.IsClosedGeneric && typeInfo.OpenGeneric == openGeneric)
            {
                yield return typeInfo;
            }
        }
    }

    public static PlTypeInfo? FindGeneric(Type openGeneric, Type innerType)
    {
        return GenericInstances(openGeneric)
            .SingleOrDefault(typeInfo => typeInfo.GenericArgument == innerType);
    }
}