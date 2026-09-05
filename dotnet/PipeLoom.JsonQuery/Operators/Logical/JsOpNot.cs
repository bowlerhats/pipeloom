using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Logical;

public class JsOpNot : PlOperatorClass
{
    public JsOpNot(IPipeLoomEngine engine)
        : base(engine, "not")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Function(Not);
    }

    public static bool Not(JsonNode? value)
    {
        return !JsonQueryUtils.IsTruthy(value);
    }
}