using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Maths;

public class JsOpPow : PlOperatorClass
{
    public JsOpPow(IPipeLoomEngine engine)
        : base(engine, "pow")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, JsonNode?>().Function(Pow);
    }

    public static JsonNode Pow(JsonNode? left, JsonNode? right)
    {
        if (left?.GetValueKind() != JsonValueKind.Number || right?.GetValueKind() != JsonValueKind.Number)
            throw new PipeLoomException("Pow expects two numbers");
        
        var leftValue = (double)left.GetValue<decimal>();
        var rightValue = (double)right.GetValue<decimal>();
        
        return rightValue == 0
            ? JsonValue.Create(1M)
            : JsonValue.Create((decimal)Math.Pow(leftValue, rightValue));
    }
}