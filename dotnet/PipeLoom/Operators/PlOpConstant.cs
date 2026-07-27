using System.Linq;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Operators;

public sealed class PlOpConstant : PlOperatorClass
{
    public override bool IsClosed => true;
    
    private OperatorHandler? NullaryHandler { get; set; }
    private OperatorHandler? UnaryHandler { get; set; }

    public PlOpConstant(IPipeLoomEngine engine)
        : base(engine, "const")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.Nullary(GetNodeConstant, static d => d.ReturnAs(NarrowToImplicit));
        registrator.Unary(GetNodeConstant, static d => d.ReturnAs(NarrowToUnaryChild));

        this.NullaryHandler = this.Handlers.Single(d => d.Arity == PlOperatorArity.Nullary);
        this.UnaryHandler = this.Handlers.Single(d => d.Arity == PlOperatorArity.Unary);
    }

    private static PlTypeDef NarrowToImplicit(WeaveNode node, PlTypeDef _)
    {
        return node.ImplicitValue.Tag as PlTypeDef
            ?? throw new PipeLoomException("Node constant does not have a type");
    }

    private static PlTypeDef NarrowToUnaryChild(WeaveNode node, PlTypeDef _)
    {
        return node.Children.Single().ReturnType;
    }

    private static Variant GetNodeConstant(in WeaveStep step)
    {
        return step.Node.ImplicitValue;
    }

    private static Variant GetNodeConstant(in Variant v)
    {
        return v;
    }

    public override OperatorHandler ChooseHandler(WeaveNode node)
    {
        return node.Arguments.Count() switch
        {
            0 => this.NullaryHandler,
            1 => this.UnaryHandler,
            _ => throw new PipeLoomException("Too much const operator arguments")
        } ?? throw new PipeLoomException("Missing const op handler");
    }
}