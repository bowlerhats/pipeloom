using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Maths;

public class JsOpSize : PlOperatorClass
{
    public JsOpSize(IPipeLoomEngine engine)
        : base(engine, "size")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Mapper(Size);
    }

    public static JsonNode Size(JsonNode? data)
    {
        return data?.GetValueKind() switch
        {
            JsonValueKind.Array => JsonValue.Create((decimal)data.AsArray().Count),
            JsonValueKind.String => JsonValue.Create(data.GetValue<string?>()?.Length ?? 0),
            _ => throw new PipeLoomException("size() expectes an array or string")
        };
    }
}