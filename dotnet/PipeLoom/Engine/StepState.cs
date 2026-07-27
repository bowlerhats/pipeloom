using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Pools;

namespace PipeLoom.Engine;

internal sealed class StepState : IStepState
{
    public WeaveContext? Context { get; private set; }
    public WeaveNode? Node { get; private set;}
    public StepState? Parent { get; private set; }

    public Variant Carry { get; set; } = Variant.Undefined;
    
    public IPoolSet PoolSet => this.Context?.Pools ?? throw new PipeLoomException("Unbound step state");

    IWeaveContext IStepState.Context => this.Context ?? throw new PipeLoomException("Unbound step state");
    IWeaveNode IStepState.Node => this.Node ?? throw new PipeLoomException("Unbound step state");

    public void Bind(WeaveContext context, WeaveNode node, StepState? parent)
    {
        this.Context = context;
        this.Node = node;
        this.Parent = parent;
        this.Carry = parent?.Carry ?? Variant.Undefined;
    }

    public void Unbind()
    {
        this.Carry = Variant.Undefined;
        this.Parent = null;
        this.Node = null;
        this.Context = null;
    }
}