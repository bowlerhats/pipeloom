namespace PipeLoom.Operators.Abstractions.Handlers;

public sealed class VariadicHandler : OperatorHandler<VariadicHandler>
{
    private VariadicHandler(PlOperatorClass operatorClass)
        : base(operatorClass, PlOperatorArity.Variadic)
    {
    }

    internal VariadicHandler(PlOperatorClass operatorClass, PlOperatorDelegates.VariadicFunction op)
        : this(operatorClass)
    {
    }
    
    internal VariadicHandler(PlOperatorClass operatorClass, PlOperatorDelegates.VariadicFunctionWithStep op)
        : this(operatorClass)
    {
        this.IsUsingStep = true;
    }
    
    internal VariadicHandler(PlOperatorClass operatorClass, PlOperatorDelegates.VariadicFunctionAsync op)
        : this(operatorClass)
    {
        this.IsAsync = true;
    }
    
    internal VariadicHandler(PlOperatorClass operatorClass, PlOperatorDelegates.VariadicFunctionAsyncWithStep op)
        : this(operatorClass)
    {
        this.IsAsync = true;
        this.IsUsingStep = true;
    }
    
    public VariadicHandler ChangeSignature<TVariadic, TReturn>()
    {
        this.Signature = HandlerSignature.Variadic<TVariadic, TReturn>(this.Engine);
        
        return this;
    }
}