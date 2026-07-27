using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Engine;

public sealed class OperatorRegistry
{
    public IPipeLoomEngine Engine { get; }
    
    public OperatorRegistry(IPipeLoomEngine engine)
    {
        this.Engine = engine;
    }
}