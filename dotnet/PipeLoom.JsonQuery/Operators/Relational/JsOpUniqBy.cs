using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.JsonQuery.Operators.Logical;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Relational;

public class JsOpUniqBy : PlOperatorClass
{
    public JsOpUniqBy(IPipeLoomEngine engine)
        : base(engine, "uniqBy")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, Detached<JsonNode?>>().Mapper(UniqBy);
    }

    public static async ValueTask<JsonNode> UniqBy(WeaveStep step, JsonNode? data, Detached<JsonNode?> keyGetter)
    {
        if (data?.GetValueKind() != JsonValueKind.Array)
            throw new PipeLoomException("uniqBy() expects an array");

        var jsArray = data.AsArray();

        var items = new Dictionary<JsonNode, JsonNode?>(JsOpEq.DeepEqualityComparer.Instance);
        
        foreach (var item in jsArray)
        {
            var key = await step.State.Step(keyGetter, item);

            if (key is null)
                throw new PipeLoomException("uniqBy() key should not be null");
            
            items.TryAdd(key, item);
        }
        
        var res = new JsonArray();
        foreach (var item in items.Values)
        {
            res.Add(item?.DeepClone());
        }

        return res;
    }
}