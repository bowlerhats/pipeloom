using PipeLoom.Engine.Pools;

namespace PipeLoom.Engine.Abstractions;

public interface IStepState
{
    IWeaveContext Context { get; }
    IWeaveNode Node { get; }
    
    Variant Carry { get; }
    
    // Bundle<Variant> CurrentBundle {get;}
    // Many<Variant> CurrentLeaf { get; }
    
    internal IPoolSet PoolSet { get; }
}
