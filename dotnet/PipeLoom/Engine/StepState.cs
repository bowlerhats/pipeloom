using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Pools;

namespace PipeLoom.Engine;

internal sealed class StepState : IStepState
{
    public WeaveContext Context => _context ?? throw new PipeLoomException("State is not bound");
    public WeaveNode Node => _node ?? throw new PipeLoomException("State is not bound");
    public StepState? Parent { get; private set; }

    public Variant Carry
    {
        get => (field.IsUndefined ? this.Parent?.Carry : field) ?? Variant.Undefined; 
        set;
    }
    
    
    public IPoolSet PoolSet => this.Context.Pools;
    
    IWeaveContext IStepState.Context => this.Context;
    IWeaveNode IStepState.Node => this.Node;

    private WeaveContext? _context;
    private WeaveNode? _node;

    public void Bind(WeaveContext context, WeaveNode node, StepState? parent)
    {
        _context = context;
        _node = node;
        this.Parent = parent;
        this.Carry = parent?.Carry ?? Variant.Undefined;
    }

    public void Unbind()
    {
        this.Carry = Variant.Undefined;
        this.Parent = null;
        _node = null;
        _context = null;
    }
    
    public ValueTask<T> Step<T>(Detached<T> detached)
    {
        return this.Context.StepDetached(detached, Variant.Undefined, this);
    }

    public ValueTask<T> Step<T, TCarry>(Detached<T> detached, TCarry carry)
    {
        return this.Context.StepDetached(detached, carry, this);
    }
}