using System;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;

namespace PipeLoom.Operators.Core;

public class PlOpLog : PlOperatorClass
{
    public override bool IsVoid => true;

    public PlOpLog(IPipeLoomEngine engine)
        : base(engine, "log")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<string?>().Function(Log);
    }

    private static Variant Log(string? s)
    {
        if (s is not null)
        {
            Console.WriteLine(s);
        }

        return Variant.Undefined;
    }
}