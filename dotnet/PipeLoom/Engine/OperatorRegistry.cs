using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Core;

namespace PipeLoom.Engine;

internal sealed class OperatorRegistry
{
    public IPipeLoomEngine Engine { get; }
    
    private FrozenDictionary<string, PlOperatorClass> _opClasses = FrozenDictionary<string, PlOperatorClass>.Empty;
    
    public OperatorRegistry(IPipeLoomEngine engine, IEngineConfig config)
    {
        this.Engine = engine;
        
        this.Build(config);
    }

    public PlOperatorClass GetOperatorClass(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        return _opClasses.TryGetValue(name, out var opClass)
            ? opClass
            : throw new PipeLoomException($"Operator class '{name}' does not exist");
    }

    private void Build(IEngineConfig config)
    {
        Dictionary<PlOperatorClass, PlOperatorRegistrator> registrators = [];
        Dictionary<string, PlOperatorClass> opClasses = [];
        
        foreach (var opClassFactory in config.OperatorClassFactories)
        {
            var opClass = opClassFactory(this.Engine);
            
            if (string.IsNullOrWhiteSpace(opClass.Name))
                throw new PipeLoomException("Operator class must have a name");
            
            opClasses[opClass.Name] = opClass;
        }

        foreach (var opClass in opClasses.Values.Distinct())
        {
            var registrator = new OperatorRegistrator(opClass);
            registrators[opClass] = registrator;
            
            opClass.RegisterHandlers(registrator);
        }

        foreach (var (name, opFactory) in config.OperatorFactories)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new PipeLoomException("Operator must have a name");
            
            if (!opClasses.TryGetValue(name, out var opClass))
            {
                opClass = new PlDynamicOpClass(this.Engine, name);
                opClasses[name] = opClass;
            }

            if (opClass.IsClosed)
                throw new PipeLoomException($"Attempted to add operators to a closed operator class '{opClass.Name}' in {opClass.GetType().FullName}");

            if (!registrators.TryGetValue(opClass, out var registrator))
            {
                registrator = new OperatorRegistrator(opClass);
                registrators[opClass] = registrator;
            }

            opFactory(registrator);
        }

        _opClasses = opClasses.ToFrozenDictionary();
    }
}