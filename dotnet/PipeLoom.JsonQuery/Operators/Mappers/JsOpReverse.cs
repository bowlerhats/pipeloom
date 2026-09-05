using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Mappers;

public class JsOpReverse: PlOperatorClass
{
    public JsOpReverse(IPipeLoomEngine engine)
        : base(engine, "reverse")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);
        
        registrator.AsUnary<JsonNode?>().Mapper(Reverse);
    }

    public static JsonNode? Reverse(JsonNode? data)
    {
        if (data?.GetValueKind() != JsonValueKind.Array)
            return data;
        var jsArray = data.AsArray();

        var res = new JsonArray();

        var reversed = jsArray.Reverse();
        foreach (var item in reversed)
        {
            res.Add(item?.DeepClone());
        }

        return res;
    }
}