using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Control;

public class JsOpIf : PlOperatorClass
{
    public JsOpIf(IPipeLoomEngine engine)
        : base(engine, "if")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsTernary<JsonNode?, JsonNode?, JsonNode?>().Function(If);
    }

    private static JsonNode? If(JsonNode? condition, JsonNode? then, JsonNode? @else)
    {
        return JsonQueryUtils.IsTruthy(condition) ? then : @else;
    }
}