using System;
using System.Collections.Generic;
using JetBrains.Annotations;

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
}