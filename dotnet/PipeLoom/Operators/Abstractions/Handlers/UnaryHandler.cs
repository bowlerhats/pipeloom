using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Adapters;

namespace PipeLoom.Operators.Abstractions.Handlers;

public sealed class UnaryHandler: OperatorHandler<UnaryHandler>
{
    public UnaryHandler(PlOperatorClass operatorClass, MethodAdapter adapter)
        : base(operatorClass, PlOperatorArity.Unary, adapter)
    {
    }

    public UnaryHandler ChangeSignature<T1, TReturn>()
    {
        this.Signature = HandlerSignature.Unary<T1, TReturn>(this.Engine);
        
        return this;
    }
}