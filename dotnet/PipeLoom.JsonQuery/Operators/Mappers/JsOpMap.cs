using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Mappers;

public class JsOpMap : PlOperatorClass
{
    public JsOpMap(IPipeLoomEngine engine)
        : base(engine, "map")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);
        
        registrator.AsBinary<JsonNode?, Detached<JsonNode?>>().Mapper(Map);
    }

    private static async ValueTask<JsonNode?> Map(WeaveStep step, JsonNode? node, Detached<JsonNode?> mapper)
    {
        if (node?.GetValueKind() != JsonValueKind.Array)
            throw new PipeLoomException("Map expects an array");

        var jsArray = node.AsArray();
        var res = new JsonArray();

        foreach (var item in jsArray)
        {
            var mapped = await step.State.Step(mapper, item);
            res.Add(mapped?.DeepClone());
        }

        return res;
    }
}