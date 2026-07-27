using System;
using System.Collections.Generic;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;

namespace PipeLoom.Engine;

internal interface IEngineConfig
{
    public List<Func<IPipeLoomEngine, PlTypeDef>> TypeFactories { get; }
    
    public List<Func<IPipeLoomEngine, PlOperatorClass>> OperatorClassFactories { get; }
    
    public List<(string name, Func<PlOperatorRegistrator, PlOperatorRegistrator> regFunc)> OperatorFactories { get; }
    
    
}