namespace PipeLoom.Operators.Abstractions.Handlers;

public sealed class UnaryHandler: OperatorHandler<UnaryHandler>
{
    private UnaryHandler(PlOperatorClass operatorClass)
        : base(operatorClass, PlOperatorArity.Unary)
    {
    }

    internal UnaryHandler(PlOperatorClass operatorClass, PlOperatorDelegates.UnaryFunction op)
        : this(operatorClass)
    {
    }
    
    internal UnaryHandler(PlOperatorClass operatorClass, PlOperatorDelegates.UnaryFunctionWithStep op)
        : this(operatorClass)
    {
        this.IsUsingStep = true;
    }
    
    internal UnaryHandler(PlOperatorClass operatorClass, PlOperatorDelegates.UnaryFunctionAsync op)
        : this(operatorClass)
    {
        this.IsAsync = true;
    }
    
    internal UnaryHandler(PlOperatorClass operatorClass, PlOperatorDelegates.UnaryFunctionAsyncWithStep op)
        : this(operatorClass)
    {
        this.IsAsync = true;
        this.IsUsingStep = true;
    }

    public UnaryHandler ChangeSignature<T1, TReturn>()
    {
        this.Signature = HandlerSignature.Unary<T1, TReturn>(this.Engine);
        
        return this;
    }
}