using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Relational;

public class JsOpFilter : PlOperatorClass
{
    public JsOpFilter(IPipeLoomEngine engine)
        : base(engine, "filter")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, Detached<JsonNode?>>().Mapper(Filter);
    }

    private static async ValueTask<JsonNode?> Filter(WeaveStep step, JsonNode? node, Detached<JsonNode?> condition)
    {
        if (node is null)
            return null;
        
        if (node.GetValueKind() != JsonValueKind.Array)
            throw new PipeLoomException("Filter expects an array input");

        var jsArray = node.AsArray();
        var res = new JsonArray();

        if (jsArray.Count > 0)
        {
            foreach (var item in jsArray)
            {
                var testResult = await step.State.Step(condition, item);
                if (JsonQueryUtils.IsTruthy(testResult))
                {
                    res.Add(item?.DeepClone());
                }
            }
        }

        return res;
    }
}