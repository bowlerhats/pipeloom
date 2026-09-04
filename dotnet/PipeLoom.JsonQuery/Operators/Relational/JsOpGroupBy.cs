using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Relational;

public class JsOpGroupBy : PlOperatorClass
{
    public JsOpGroupBy(IPipeLoomEngine engine)
        : base(engine, "groupBy")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, Detached<JsonNode?>>().Mapper(GroupBy);
    }

    public static async ValueTask<JsonNode> GroupBy(WeaveStep step, JsonNode? data, Detached<JsonNode?> keyGetter)
    {
        if (data?.GetValueKind() != JsonValueKind.Array)
            throw new PipeLoomException("GroupBy expects an array of objects");

        var jsArray = data.AsArray();

        var res = new JsonObject();

        foreach (var item in jsArray)
        {
            if (item?.GetValueKind() != JsonValueKind.Object)
                throw new PipeLoomException("GroupBy expects an array of objects");

            var key = await step.State.Step(keyGetter, item);

            var gKey = key?.GetValueKind() switch
            {
                JsonValueKind.Number => key.GetValue<decimal>().ToString(CultureInfo.InvariantCulture),
                JsonValueKind.String => key.GetValue<string>(),
                _ => throw new PipeLoomException("Grouping key expected to be number or string")
            };
            
            JsonArray gArray;
            if (!res.ContainsKey(gKey))
            {
                gArray = new JsonArray();
                res[gKey] = gArray;
            }
            else
            {
                gArray = res[gKey]!.AsArray();
            }
            
            gArray.Add(item?.DeepClone());
        }

        return res;
    }
}