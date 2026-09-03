using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Maths;

public class JsOpDivide : PlOperatorClass
{
    public JsOpDivide(IPipeLoomEngine engine)
        : base(engine, "divide")
    {
    }
    
    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, JsonNode?>().Function(Divide);
    }

    public static JsonNode Divide(JsonNode? left, JsonNode? right)
    {
        if (left?.GetValueKind() != JsonValueKind.Number || right?.GetValueKind() != JsonValueKind.Number)
            throw new PipeLoomException("Division expects two numbers");
        
        var leftValue = left.GetValue<decimal>();
        var rightValue = right.GetValue<decimal>();
        
        return rightValue == 0
            ? throw new PipeLoomException("Division by zero")
            : JsonValue.Create(leftValue / rightValue);
    }
}