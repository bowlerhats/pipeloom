using PipeLoom.Engine.Abstractions.Adapters;

namespace PipeLoom.Operators.Abstractions.Handlers;

public sealed class VariadicHandler : OperatorHandler<VariadicHandler>
{
    public VariadicHandler(PlOperatorClass operatorClass, MethodAdapter adapter)
        : base(operatorClass, PlOperatorArity.Variadic, adapter)
    {
    }
    
    public VariadicHandler ChangeSignature<TVariadic, TReturn>()
    {
        this.Signature = HandlerSignature.Variadic<TVariadic, TReturn>(this.Engine);
        
        return this;
    }
}