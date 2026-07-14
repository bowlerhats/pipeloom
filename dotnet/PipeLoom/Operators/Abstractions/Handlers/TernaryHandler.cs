namespace PipeLoom.Operators.Abstractions.Handlers;

public sealed class TernaryHandler : OperatorHandler<TernaryHandler>
{
    private TernaryHandler(PlOperatorClass operatorClass)
        : base(operatorClass, PlOperatorArity.Ternary)
    {
    }

    internal TernaryHandler(PlOperatorClass operatorClass, PlOperatorDelegates.TernaryFunction op)
        : this(operatorClass)
    {
    }
    
    internal TernaryHandler(PlOperatorClass operatorClass, PlOperatorDelegates.TernaryFunctionWithStep op)
        : this(operatorClass)
    {
        this.IsUsingStep = true;
    }
    
    internal TernaryHandler(PlOperatorClass operatorClass, PlOperatorDelegates.TernaryFunctionAsync op)
        : this(operatorClass)
    {
        this.IsAsync = true;
    }
    
    internal TernaryHandler(PlOperatorClass operatorClass, PlOperatorDelegates.TernaryFunctionAsyncWithStep op)
        : this(operatorClass)
    {
        this.IsAsync = true;
        this.IsUsingStep = true;
    }
    
    public TernaryHandler ChangeSignature<T1, T2, T3, TReturn>()
    {
        this.Signature = HandlerSignature.Ternary<T1, T2, T3, TReturn>(this.Engine);
        
        return this;
    }
}