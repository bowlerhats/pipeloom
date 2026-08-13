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

    IBundle<T> NewBundle<T>();

    // ValueTask<T> Step<T>(scoped in Detached<T> detached);
    // ValueTask<T> Step<T, TCarry>(scoped in Detached<T> detached, TCarry carry);
}
