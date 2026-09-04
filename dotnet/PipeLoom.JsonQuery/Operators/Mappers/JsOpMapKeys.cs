using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Mappers;

public class JsOpMapKeys: PlOperatorClass
{
    public JsOpMapKeys(IPipeLoomEngine engine)
        : base(engine, "mapKeys")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, Detached<JsonNode?>>().Mapper(MapKeys);
    }

    public static async ValueTask<JsonNode?> MapKeys(WeaveStep step, JsonNode? data, Detached<JsonNode?> projector)
    {
        if (data?.GetValueKind() != JsonValueKind.Object)
            throw new PipeLoomException("mapKeys expects an object");

        var jsObject = data.AsObject();

        var res = new JsonObject();

        foreach (var (key, value) in jsObject)
        {
            var newKeyNode = await step.State.Step(projector, (JsonNode)JsonValue.Create(key));
            
            switch (newKeyNode?.GetValueKind())
            {
                case JsonValueKind.Number:
                    res[(int)newKeyNode.GetValue<decimal>()] = value?.DeepClone();
                    break;
                case JsonValueKind.String:
                    res[newKeyNode.GetValue<string>()] = value?.DeepClone();
                    break;
                default:
                    throw new PipeLoomException("Key mapping must result in either a new number or a new string key");
            }
        }

        return res;
    }
}