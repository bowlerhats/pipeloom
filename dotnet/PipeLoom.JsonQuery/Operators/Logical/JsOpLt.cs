using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Logical;

public class JsOpLt : PlOperatorClass
{
    public JsOpLt(IPipeLoomEngine engine)
        : base(engine, "lt")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, JsonNode?>().Function(Lt);
    }

    public static bool Lt(JsonNode? left, JsonNode? right)
    {
        return !JsOpGte.Gte(left, right);
    }
}