using System;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Adapters;

namespace PipeLoom.Operators.Abstractions.Handlers;

public abstract class OperatorHandler
{
    public IPipeLoomEngine Engine { get; }
    
    public PlOperatorClass OperatorClass { get; }
    
    public PlOperatorArity Arity { get; }
    
    public HandlerSignature Signature { get; protected set; }

    public HandlerRole Role { get; set; } = HandlerRole.None;
    
    public MethodAdapter Adapter { get; }
    
    protected Func<WeaveNode, PlTypeDef, PlTypeDef>? Narrower { get; set; }
    
    protected OperatorHandler(
        PlOperatorClass operatorClass,
        PlOperatorArity arity,
        MethodAdapter adapter
        )
    {
        this.OperatorClass = operatorClass;
        this.Engine = operatorClass.Engine;
        this.Arity = arity;
        this.Adapter = adapter;

        this.Signature = arity switch
        {
            PlOperatorArity.Nullary => HandlerSignature.Nullary<Variant>(this.Engine),
            PlOperatorArity.Unary => HandlerSignature.Unary<Variant, Variant>(this.Engine),
            PlOperatorArity.Binary => HandlerSignature.Binary<Variant, Variant, Variant>(this.Engine),
            PlOperatorArity.Ternary => HandlerSignature.Ternary<Variant, Variant, Variant, Variant>(this.Engine),
            PlOperatorArity.Variadic => HandlerSignature.Variadic<Variant, Variant>(this.Engine),
            _ => throw new ArgumentOutOfRangeException(nameof(arity), arity, null)
        };
    }

    //public abstract ValueTask<Variant> Call(IStepState state, scoped in ReadOnlyMemory<Variant> arguments);

    public PlTypeDef NarrowReturnType(WeaveNode node)
    {
        var @implicit = this.ImplicitNarrow(node);

        return this.Narrower?.Invoke(node, @implicit) ?? @implicit;
    }

    public PlTypeDef ImplicitNarrow(WeaveNode node)
    {
        if (node.NarrowedReturnType is not null)
        {
            return node.NarrowedReturnType.IsAssignableTo(this.Signature.ReturnType)
                ? node.NarrowedReturnType
                : this.Signature.ReturnType;
        }
        
        return this.Signature.ReturnType;
    }
}

public abstract class OperatorHandler<TSelf> : OperatorHandler
    where TSelf: OperatorHandler<TSelf>
{
    public TSelf Self => (TSelf)this;
    
    protected OperatorHandler(PlOperatorClass operatorClass, PlOperatorArity arity, MethodAdapter adapter)
        : base(operatorClass, arity, adapter)
    {
    }
    
    public TSelf WithRole(HandlerRole role)
    {
        this.Role = role;
        return this.Self;
    }

    public TSelf ReturnAs(Func<WeaveNode, PlTypeDef, PlTypeDef> narrower)
    {
        this.Narrower = narrower;
        return this.Self;
    }
}