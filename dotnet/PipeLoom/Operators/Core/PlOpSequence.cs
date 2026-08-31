using System;
using System.Linq;
using System.Threading.Tasks;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Operators.Core;

public sealed class PlOpSequence : PlOperatorClass
{
    public override bool IsClosed => true;
    
    public PlOpSequence(IPipeLoomEngine engine)
        : base(engine, "sequence")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        registrator.Variadic(RunSequence, h => h.ReturnAs(Narrow));
    }
    
    private static Variant RunSequence(scoped in ReadOnlyMemory<Variant> args)
    {
        return args.Length > 0 ? args.Span[^1] : Variant.Undefined;
    }
    
    private static PlTypeDef Narrow(WeaveNode node, PlTypeDef _)
    {
        var last = node.Arguments.LastOrDefault();

        return last?.ReturnType ?? node.Engine.WellKnown.Void;
    }

    public override ValueTask<PreFuseFlags> PreFuse(WeaveNode node)
    {
        var last = node.Arguments.LastOrDefault();
        node.IsVoid = last is null;
        
        if (node is { IsVoid: true, RequiredReturnType: PlVariant })
            return base.PreFuse(node);

        if (node.RequiredReturnType is not null)
        {
            if (node.IsVoid || last is null)
                throw new PipeLoomException("Sequence has an unfulfillable return requirement");

            last.RequiredReturnType = node.RequiredReturnType;
        }

        return base.PreFuse(node);
    }

    public override OperatorHandler ChooseHandler(WeaveNode node)
    {
        return this.Handlers.Single();
    }
}