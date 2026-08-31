using System;
using System.Linq;
using System.Threading.Tasks;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Operators.CoreControlFlow;

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
        
        registrator.AsVariadic<Detached<Variant>>().Function(Pipe, cfg => cfg.ReturnAs(Narrow));
    }


    private static PlTypeDef Narrow(WeaveNode node, PlTypeDef _)
    {
        var last = node.Arguments.LastOrDefault();

        return last?.ReturnType ?? node.Engine.WellKnown.Void;
    }
    
    public static async ValueTask<Variant> Pipe(WeaveStep step, ReadOnlyMemory<Detached<Variant>> children)
    {
        var carry = Variant.Undefined;

        var childCount = children.Length;
        for (var i = 0; i < childCount; i++)
        {
            var child = children.Span[i];
            var stepResult = await step.State.Step(child, carry);

            if (child.Node.IsArgument)
            {
                carry = stepResult;
            }
        }

        return carry;
    }
    
    public override async ValueTask<PreFuseFlags> PreFuse(WeaveNode node)
    {
        var carry = node.CarryType;
        
        foreach (var child in node.Children.ToList())
        {
            child.CarryType = carry;

            await child.Fuse();
            
            if (child.IsArgument)
                carry = child.ReturnType;
        }
        
        return PreFuseFlags.SkipChildFuse;
    }

    public override OperatorHandler? ChooseHandler(WeaveNode node)
    {
        return this.Handlers.Single();
    }
}