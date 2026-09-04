using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Projectors;

public class JsOpValues : PlOperatorClass
{
    public JsOpValues(IPipeLoomEngine engine)
        : base(engine, "values")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Mapper(GetValues);
    }

    public static JsonNode GetValues(JsonNode? data)
    {
        if (data?.GetValueKind() != JsonValueKind.Object)
            throw new PipeLoomException("values() expects an object");

        var jsObject = data.AsObject();

        var res = new JsonArray();

        foreach (var (_, value) in jsObject)
        {
            res.Add(value?.DeepClone());
        }

        return res;
    }
}