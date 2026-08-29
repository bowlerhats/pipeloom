using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using PipeLoom.Engine.Abstractions.Bundles;

namespace PipeLoom.Engine.Abstractions;

public interface IPipeLoomEngine : IDisposable
{
    IPlConversionMap Conversions { get; }
    
    IPlWellknown WellKnown { get; }
    
    PlTypeDef TypeOf<T>();

    PlGenericType? FindGeneric(Type nativeOpenGenericType);

    PlTypeDef CommonBaseOf(IEnumerable<PlTypeDef> types);

    PlOperatorArity? GuessArity([NoEnumeration] IEnumerable<PlTypeDef> args);

    PlOperatorClass GetOperatorClass(string operatorName);

    Variant ToVariant<T>(in T value);
    T FromVariant<T>(in Variant value);

    void Touch<T>();

    internal int NextTypeId();
    
    internal TypeMap TypeMap { get; }

    static void Discover<T>()
    {
        Discovery.Discover<T>();
    }
}