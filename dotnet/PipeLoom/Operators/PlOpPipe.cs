using System;
using System.Linq;
using System.Threading.Tasks;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Operators;

public sealed class PlOpPipe : PlOperatorClass
{
    public override bool IsClosed => true;

    public PlOpPipe(IPipeLoomEngine engine)
        : base(engine, "pipe")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsVariadic<Detached<Variant>>().Function(Pipe);
    }

    public static ValueTask<Variant> Pipe(WeaveStep step, ReadOnlyMemory<Detached<Variant>> children)
    {
        
        throw new NotImplementedException();
    }

    public override ValueTask<PreFuseFlags> PreFuse(WeaveNode node)
    {
        return base.PreFuse(node);
    }

    public override OperatorHandler? ChooseHandler(WeaveNode node)
    {
        return this.Handlers.Single();
    }
}