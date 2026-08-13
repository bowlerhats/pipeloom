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

    public Variant Carry { get; set; } = Variant.Undefined;
    
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
    
    public IBundle<T> NewBundle<T>()
    {
        return this.Context.NewBundle<T>();
    }

    public ValueTask<T> Step<T>(scoped in Detached<T> detached)
    {
        throw new System.NotImplementedException();
    }

    public ValueTask<T> Step<T, TCarry>(scoped in Detached<T> detached, TCarry carry)
    {
        throw new System.NotImplementedException();
    }
}