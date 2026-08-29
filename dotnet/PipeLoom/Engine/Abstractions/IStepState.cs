using System.Threading.Tasks;
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

    ValueTask<T> Step<T>(Detached<T> detached);
    ValueTask<T> Step<T, TCarry>(Detached<T> detached, TCarry carry);
}
