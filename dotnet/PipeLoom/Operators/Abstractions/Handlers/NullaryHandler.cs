namespace PipeLoom.Operators.Abstractions.Handlers;

public sealed class NullaryHandler : OperatorHandler<NullaryHandler>
{
    private NullaryHandler(PlOperatorClass operatorClass)
        : base(operatorClass, PlOperatorArity.Nullary)
    {
    }

    internal NullaryHandler(PlOperatorClass operatorClass, PlOperatorDelegates.NullaryFunction op)
        : this(operatorClass)
    {
    }
    
    internal NullaryHandler(PlOperatorClass operatorClass, PlOperatorDelegates.NullaryFunctionWithStep op)
        : this(operatorClass)
    {
        this.IsUsingStep = true;
    }
    
    internal NullaryHandler(PlOperatorClass operatorClass, PlOperatorDelegates.NullaryFunctionAsync op)
        : this(operatorClass)
    {
        this.IsAsync = true;
    }
    
    internal NullaryHandler(PlOperatorClass operatorClass, PlOperatorDelegates.NullaryFunctionAsyncWithStep op)
        : this(operatorClass)
    {
        this.IsAsync = true;
        this.IsUsingStep = true;
    }

    public NullaryHandler ChangeSignature<TReturn>()
    {
        this.Signature = HandlerSignature.Nullary<TReturn>(this.Engine);
        
        return this;
    }
}