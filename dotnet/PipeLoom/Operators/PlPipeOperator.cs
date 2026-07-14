using System;
using System.Threading.Tasks;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;

namespace PipeLoom.Operators;

public class PlOpPipe : PlOperatorClass
{
    public PlOpPipe(IPipeLoomEngine engine)
        : base(engine, "pipe")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsVariadic<Detached<Variant>>().Bundler(Pipe);
    }

    public static ValueTask<IBundle<Variant>> Pipe(WeaveStep step, ReadOnlyMemory<Detached<Variant>> args)
    {
        throw new NotImplementedException();
    }
}