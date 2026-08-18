using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Adapters;

namespace PipeLoom.Operators.Abstractions.Handlers;

public sealed class NullaryHandler : OperatorHandler<NullaryHandler>
{
    internal NullaryHandler(PlOperatorClass operatorClass, MethodAdapter adapter)
        : base(operatorClass, PlOperatorArity.Nullary, adapter)
    {
    }

    public NullaryHandler ChangeSignature<TReturn>()
    {
        this.Engine.Touch<TReturn>();
        
        this.Signature = HandlerSignature.Nullary<TReturn>(this.Engine);
        
        return this;
    }
}