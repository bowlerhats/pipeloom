using System.Linq;
using System.Threading.Tasks;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;

namespace PipeLoom.Operators.CoreControlFlow;

public class PlOpIf : PlOperatorClass
{
    private readonly PlTypeDef _boolType;
    
    public PlOpIf(IPipeLoomEngine engine)
        : base(engine, "if")
    {
        _boolType = engine.TypeOf<bool>();
    }

    public override ValueTask<PreFuseFlags> PreFuse(WeaveNode node)
    {
        node.Children[0].RequiredReturnType = _boolType;
        
        foreach (var restArg in node.Children.Skip(1).Take(2))
        {
            restArg.IsForcedArgument = true;
        }
        
        return base.PreFuse(node);
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<bool, Detached<Variant>>()
            .Function(If, cfg => cfg.ReturnAs(static (node, _) => node.Arguments.ElementAt(1).ReturnType));

        registrator.AsTernary<bool, Detached<Variant>, Detached<Variant>>()
            .Function(If, cfg =>
                cfg.ReturnAs(static (node, _) =>
                    node.Engine.CommonBaseOf(node.Arguments.Skip(1).Select(d => d.ReturnType))
                ));
    }

    private static ValueTask<Variant> If(WeaveStep step, bool condition, Detached<Variant> then)
    {
        return condition
            ? step.State.Step(then)
            : ValueTask.FromResult(Variant.Undefined);
    }

    private static ValueTask<Variant> If(WeaveStep step, bool condition, Detached<Variant> then, Detached<Variant> @else)
    {
        return condition
            ? step.State.Step(then)
            : step.State.Step(@else);
    }
}