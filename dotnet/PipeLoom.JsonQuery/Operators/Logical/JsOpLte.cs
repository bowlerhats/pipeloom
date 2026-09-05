using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Logical;

public class JsOpLte : PlOperatorClass
{
    public JsOpLte(IPipeLoomEngine engine)
        : base(engine, "lte")
    {
    }
    
    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, JsonNode?>().Function(Lte);
    }

    public static bool Lte(JsonNode? left, JsonNode? right)
    {
        return !JsOpGt.Gt(left, right);
    }
}