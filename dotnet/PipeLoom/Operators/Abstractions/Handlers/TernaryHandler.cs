using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Adapters;

namespace PipeLoom.Operators.Abstractions.Handlers;

public sealed class TernaryHandler : OperatorHandler<TernaryHandler>
{
    public TernaryHandler(PlOperatorClass operatorClass, MethodAdapter adapter)
        : base(operatorClass, PlOperatorArity.Ternary, adapter)
    {
    }
    
    public TernaryHandler ChangeSignature<T1, T2, T3, TReturn>()
    {
        this.Signature = HandlerSignature.Ternary<T1, T2, T3, TReturn>(this.Engine);
        
        return this;
    }
}