namespace PipeLoom.Operators.Abstractions.Handlers;

public sealed class BinaryHandler: OperatorHandler<BinaryHandler>
{
    private BinaryHandler(PlOperatorClass operatorClass)
        : base(operatorClass, PlOperatorArity.Binary)
    {
    }

    internal BinaryHandler(PlOperatorClass operatorClass, PlOperatorDelegates.BinaryFunction op)
        : this(operatorClass)
    {
    }
    
    internal BinaryHandler(PlOperatorClass operatorClass, PlOperatorDelegates.BinaryFunctionWithStep op)
        : this(operatorClass)
    {
        this.IsUsingStep = true;
    }
    
    internal BinaryHandler(PlOperatorClass operatorClass, PlOperatorDelegates.BinaryFunctionAsync op)
        : this(operatorClass)
    {
        this.IsAsync = true;
    }
    
    internal BinaryHandler(PlOperatorClass operatorClass, PlOperatorDelegates.BinaryFunctionAsyncWithStep op)
        : this(operatorClass)
    {
        this.IsAsync = true;
        this.IsUsingStep = true;
    }
    
    public BinaryHandler ChangeSignature<T1, T2, TReturn>()
    {
        this.Signature = HandlerSignature.Binary<T1, T2, TReturn>(this.Engine);
        
        return this;
    }
}