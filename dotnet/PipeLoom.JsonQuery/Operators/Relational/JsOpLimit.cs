using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Relational;

public class JsOpLimit : PlOperatorClass
{
    public JsOpLimit(IPipeLoomEngine engine)
        : base(engine, "limit")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, decimal>().Mapper(Limit);
    }

    public static JsonNode Limit(JsonNode? data, decimal limit)
    {
        if (data?.GetValueKind() != JsonValueKind.Array)
            throw new PipeLoomException("limit() expects an array of objects");
        
        var iLimit = (int)limit;
        if (iLimit <= 0)
            return new JsonArray();
        
        var jsArray = data.AsArray();
        var count = 0;

        var res = new JsonArray();
        foreach (var item in jsArray)
        {
            if (++count > limit)
                break;
            
            res.Add(item?.DeepClone());
        }

        return res;
    }
}