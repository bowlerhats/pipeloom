using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Adapters;

namespace PipeLoom.Operators.Abstractions.Handlers;

public sealed class BinaryHandler: OperatorHandler<BinaryHandler>
{
    public BinaryHandler(PlOperatorClass operatorClass, MethodAdapter adapter)
        : base(operatorClass, PlOperatorArity.Binary, adapter)
    {
    }
    
    public BinaryHandler ChangeSignature<T1, T2, TReturn>()
    {
        this.Engine.Touch<T1>();
        this.Engine.Touch<T2>();
        this.Engine.Touch<TReturn>();
        
        this.Signature = HandlerSignature.Binary<T1, T2, TReturn>(this.Engine);
        
        return this;
    }
}