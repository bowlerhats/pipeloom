using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Logical;

public class JsOpGt : PlOperatorClass
{
    public JsOpGt(IPipeLoomEngine engine)
        : base(engine, "gt")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, JsonNode?>().Function(Gt);
    }

    public static bool Gt(JsonNode? left, JsonNode? right)
    {
        return (left?.GetValueKind(), right?.GetValueKind()) switch
        {
            (JsonValueKind.Number, JsonValueKind.Number) => left.GetValue<decimal>() > right.GetValue<decimal>(),
            (JsonValueKind.String, JsonValueKind.String) => string.Compare(left.GetValue<string>(), right.GetValue<string>(), StringComparison.Ordinal) > 0,
            (JsonValueKind.True, JsonValueKind.True) => false,
            (JsonValueKind.True, JsonValueKind.False) => true,
            (JsonValueKind.False, JsonValueKind.True) => false,
            (JsonValueKind.False, JsonValueKind.False) => false,
            _ => throw new PipeLoomException("gt() expects two numbers|strings|booleans")
        };
    }
}