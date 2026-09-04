using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Projectors;

public class JsOpKeys : PlOperatorClass
{
    public JsOpKeys(IPipeLoomEngine engine)
        : base(engine, "keys")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Mapper(GetKeys);
    }

    public static JsonNode GetKeys(JsonNode? data)
    {
        if (data?.GetValueKind() != JsonValueKind.Object)
            throw new PipeLoomException("keys() expects an object");

        var jsObject = data.AsObject();

        var res = new JsonArray();

        foreach (var (key, _) in jsObject)
        {
            res.Add((JsonNode)JsonValue.Create(key));
        }

        return res;
    }
}