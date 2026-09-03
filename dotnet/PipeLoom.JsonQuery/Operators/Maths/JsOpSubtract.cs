using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Maths;

public class JsOpSubtract : PlOperatorClass
{
    public JsOpSubtract(IPipeLoomEngine engine)
        : base(engine, "subtract")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);
        
        registrator.AsBinary<JsonNode?, JsonNode?>().Function(Subtract);
    }

    public static JsonNode Subtract(JsonNode? left, JsonNode? right)
    {
        return (left?.GetValueKind(), right?.GetValueKind()) switch
        {
            (JsonValueKind.Number, JsonValueKind.Number)
                => JsonValue.Create(left.GetValue<decimal>() - right.GetValue<decimal>()),
            _ => throw new PipeLoomException("Subtract expects two numbers")
        };
    }
}