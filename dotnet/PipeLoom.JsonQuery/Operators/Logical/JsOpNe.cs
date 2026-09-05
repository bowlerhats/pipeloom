using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Logical;

public class JsOpNe : PlOperatorClass
{
    public JsOpNe(IPipeLoomEngine engine)
        : base(engine, "ne")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, JsonNode?>().Function(Ne);
    }

    public static bool Ne(JsonNode? left, JsonNode? right)
    {
        return !JsOpEq.Eq(left, right);
    }
}