using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Mappers;

public class JsOpMapValues : PlOperatorClass
{
    public JsOpMapValues(IPipeLoomEngine engine)
        : base(engine, "mapValues")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode, Detached<JsonNode?>>().Mapper(MapValues);
    }

    public static async ValueTask<JsonNode?> MapValues(WeaveStep step, JsonNode? data, Detached<JsonNode?> projector)
    {
        if (data?.GetValueKind() != JsonValueKind.Object)
            throw new PipeLoomException("mapKeys expects an object");

        var jsObject = data.AsObject();

        var res = new JsonObject();

        foreach (var (key, value) in jsObject)
        {
            var newValue = await step.State.Step(projector, value);
            res[key] = newValue?.DeepClone();
        }

        return res;
    }
}