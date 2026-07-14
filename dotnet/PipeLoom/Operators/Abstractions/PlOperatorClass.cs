using System;
using System.Collections.Generic;
using System.Linq;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Operators.Abstractions;

public interface IPlOperatorClass
{
    IPipeLoomEngine Engine { get; }
    
    string Name { get; }
    IReadOnlyCollection<string> Aliases { get; }
}

public abstract class PlOperatorClass : IPlOperatorClass
{
    public string Name { get; }
    
    public virtual List<string> Aliases { get; } = [];

    IReadOnlyCollection<string> IPlOperatorClass.Aliases => this.Aliases;
    
    public IPipeLoomEngine Engine { get; }

    private List<OperatorHandler> _handlers = [];
    
    protected PlOperatorClass(IPipeLoomEngine engine, string name)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        this.Engine = engine;
        this.Name = name;
    }

    internal void AddHandler(OperatorHandler opHandler)
    {
        ArgumentNullException.ThrowIfNull(opHandler);
        if (opHandler.OperatorClass != this)
        {
            throw new ArgumentException(
                $"Attempted to add handler of class '{opHandler.OperatorClass.Name}' to op class '{this.Name}'",
                nameof(opHandler));
        }

        if (_handlers.Any(d => d.Signature.Equals(opHandler.Signature)))
        {
            throw new ArgumentOutOfRangeException(
                $"Operator class '{this.Name}' already contains a handler with same signature '{opHandler.Signature}'");
        }
        
        _handlers.Add(opHandler);
    }
    
    public virtual void RegisterHandlers(PlOperatorRegistrator registrator)
    {
    }
}
