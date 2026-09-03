using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Maths;

public class JsOpRound : PlOperatorClass
{
    public JsOpRound(IPipeLoomEngine engine)
        : base(engine, "round")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Function(Round);
        registrator.AsBinary<JsonNode?, decimal>().Function(Round);
    }

    public static JsonNode Round(JsonNode? value)
    {
        if (value?.GetValueKind() != JsonValueKind.Number)
            throw new PipeLoomException("Abs expects a number");
        
        return JsonValue.Create(Math.Round(value.GetValue<decimal>()));
    }
    
    public static JsonNode Round(JsonNode? value, decimal digits)
    {
        if (value?.GetValueKind() != JsonValueKind.Number)
            throw new PipeLoomException("Abs expects a number");
        
        return JsonValue.Create(Math.Round(value.GetValue<decimal>(), (int)digits));
    }
}