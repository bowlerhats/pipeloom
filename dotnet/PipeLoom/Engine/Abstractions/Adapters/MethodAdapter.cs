using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PipeLoom.Engine.Abstractions.Adapters;

public abstract partial class MethodAdapter
{
    protected delegate ValueTask<Variant> MethodCaller(IStepState stepState, ReadOnlyMemory<Variant> arguments);
    
    public IPipeLoomEngine Engine { get; }
    
    public abstract PlOperatorArity Arity { get; }

    protected MethodCaller Caller { get; set; } = null!;
    
    protected MethodAdapter(IPipeLoomEngine engine)
    {
        this.Engine = engine;
    }
    
    public ValueTask<Variant> Call(IStepState stepState, scoped in ReadOnlyMemory<Variant> arguments)
    {
        Debug.Assert(this.Caller is not null, "MethodAdapter is missing an initialized caller. Was Seal() called?!");
        
        return this.Caller(stepState, arguments);
    }

    protected void Seal(MethodCaller caller)
    {
        ArgumentNullException.ThrowIfNull(caller);
        
        this.Caller = caller;
    }
}