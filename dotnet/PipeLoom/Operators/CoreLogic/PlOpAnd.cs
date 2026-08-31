using System;
using System.Threading.Tasks;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;

namespace PipeLoom.Operators.CoreLogic;

public class PlOpAnd : PlOperatorClass
{
    private readonly PlTypeDef _boolType;
    
    public PlOpAnd(IPipeLoomEngine engine)
        : base(engine, "and")
    {
        _boolType = engine.TypeOf<bool>();
    }

    public override ValueTask<PreFuseFlags> PreFuse(WeaveNode node)
    {
        foreach (var arg in node.Arguments)
        {
            arg.RequiredReturnType = _boolType;
        }
        
        return base.PreFuse(node);
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<Detached<bool>>().Function(And);
        registrator.AsVariadic<Detached<bool>>().Function(And);
    }

    private static async ValueTask<bool> And(WeaveStep step, Detached<bool> left, Detached<bool> right)
    {
        var leftResult = await step.State.Step(left);
        if (!leftResult)
            return false;
        
        return await step.State.Step(right);
    }
    
    private static async ValueTask<bool> And(WeaveStep step, ReadOnlyMemory<Detached<bool>> args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args.Span[i];

            if (!await step.State.Step(arg))
                return false;
        }

        return true;
    }
}