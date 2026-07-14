using System;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Operators.Abstractions.Handlers;

public abstract class OperatorHandler
{
    public IPipeLoomEngine Engine { get; }
    
    public PlOperatorClass OperatorClass { get; }
    
    public PlOperatorArity Arity { get; }
    
    public HandlerSignature Signature { get; protected set; }

    public HandlerRole Role { get; set; } = HandlerRole.None;
    
    public bool IsAsync { get; protected set; }
    
    public bool IsUsingStep { get; protected set; }
    
    protected OperatorHandler(PlOperatorClass operatorClass, PlOperatorArity arity)
    {
        this.OperatorClass = operatorClass;
        this.Engine = operatorClass.Engine;
        this.Arity = arity;

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
}

public abstract class OperatorHandler<TSelf> : OperatorHandler
    where TSelf: OperatorHandler<TSelf>
{
    public TSelf Self => (TSelf)this;
    
    protected OperatorHandler(PlOperatorClass operatorClass, PlOperatorArity arity)
        : base(operatorClass, arity)
    {
    }
    
    public TSelf WithRole(HandlerRole role)
    {
        this.Role = role;
        return this.Self;
    }
}