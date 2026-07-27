using PipeLoom.Engine.Abstractions.Adapters;

namespace PipeLoom.Operators.Abstractions.Handlers;

public sealed class VariadicHandler : OperatorHandler<VariadicHandler>
{
    public bool IsHomogenic { get; private set; }
    
    public VariadicHandler(PlOperatorClass operatorClass, MethodAdapter adapter)
        : base(operatorClass, PlOperatorArity.Variadic, adapter)
    {
    }
    
    public VariadicHandler ChangeSignature<TVariadic, TReturn>()
    {
        this.Signature = HandlerSignature.Variadic<TVariadic, TReturn>(this.Engine);
        this.IsHomogenic = true;
        
        return this;
    }
    
    public VariadicHandler ChangeSignature<TImplicit, TVariadic, TReturn>()
    {
        this.Signature = HandlerSignature.Variadic<TImplicit, TVariadic, TReturn>(this.Engine);
        this.IsHomogenic = false;
        
        return this;
    }
}