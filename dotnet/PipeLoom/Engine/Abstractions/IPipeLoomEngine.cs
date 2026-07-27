using System.Buffers;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace PipeLoom.Engine.Abstractions;

public interface IPipeLoomEngine
{
    IPlConversionMap Conversions { get; }
    
    IPlWellknown WellKnown { get; }
    
    PlTypeDef TypeOf<T>();

    PlTypeDef CommonBaseOf(IEnumerable<PlTypeDef> types);

    PlOperatorArity? GuessArity([NoEnumeration] IEnumerable<PlTypeDef> args);

    PlOperatorClass GetOperatorClass(string operatorName);
    
    
}