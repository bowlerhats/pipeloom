using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Maths;

public class JsOpAbs : PlOperatorClass
{
    public JsOpAbs(IPipeLoomEngine engine)
        : base(engine, "abs")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Function(Abs);
    }

    public static JsonNode Abs(JsonNode? value)
    {
        if (value?.GetValueKind() != JsonValueKind.Number)
            throw new PipeLoomException("Abs expects a number");
        
        return JsonValue.Create(Math.Abs(value.GetValue<decimal>()));
    }
}