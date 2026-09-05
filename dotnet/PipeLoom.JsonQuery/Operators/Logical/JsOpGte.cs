using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Logical;

public class JsOpGte : PlOperatorClass
{
    public JsOpGte(IPipeLoomEngine engine)
        : base(engine, "gte")
    {
    }
    
    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, JsonNode?>().Function(Gte);
    }

    public static bool Gte(JsonNode? left, JsonNode? right)
    {
        return (left?.GetValueKind(), right?.GetValueKind()) switch
        {
            (JsonValueKind.Number, JsonValueKind.Number)
                => left.GetValue<decimal>() >= right.GetValue<decimal>(),
            (JsonValueKind.String, JsonValueKind.String)
                => string.Compare(left.GetValue<string>(), right.GetValue<string>(), StringComparison.Ordinal) >= 0,
            (JsonValueKind.True, JsonValueKind.True) => true,
            (JsonValueKind.True, JsonValueKind.False) => true,
            (JsonValueKind.False, JsonValueKind.True) => false,
            (JsonValueKind.False, JsonValueKind.False) => true,
            _ => false
        };
    }
}