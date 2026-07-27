using PipeLoom.Engine.Pools;

namespace PipeLoom.Engine.Abstractions;

public readonly record struct WeaveStep(IStepState State)
{
    public IWeaveNode Node => this.State.Node;
    public IPoolSet Pools => this.State.Context.Pools;
}