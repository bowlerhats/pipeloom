using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Relational;

public class JsOpFlatten : PlOperatorClass
{
    public JsOpFlatten(IPipeLoomEngine engine)
        : base(engine, "flatten")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Mapper(Flatten);
    }

    public static JsonNode Flatten(JsonNode? data)
    {
        if (data?.GetValueKind() != JsonValueKind.Array)
            throw new PipeLoomException("flatten() expectes an array");

        var jsArray = data.AsArray();

        var res = new JsonArray();

        foreach (var item in jsArray)
        {
            switch (item?.GetValueKind())
            {
                case JsonValueKind.Array:
                    foreach (var subItem in item.AsArray())
                    {
                        res.Add(subItem?.DeepClone());
                    }
                    break;
                default:
                    res.Add(item?.DeepClone());
                    break;
            }
        }

        return res;
    }
}