using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Mappers;

public class JsOpMapObject : PlOperatorClass
{
    public JsOpMapObject(IPipeLoomEngine engine)
        : base(engine, "mapObject")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, Detached<JsonNode?>>().Mapper(MapObject);
    }

    public static async ValueTask<JsonNode?> MapObject(WeaveStep step, JsonNode? data, Detached<JsonNode?> projector)
    {
        if (data?.GetValueKind() != JsonValueKind.Object)
            throw new PipeLoomException("mapObject expects an object");

        var jsObject = data.AsObject();

        var res = new JsonObject();

        foreach (var (key, value) in jsObject)
        {
            var item = new JsonObject
            {
                ["key"] = key,
                ["value"] = value?.DeepClone()
            };

            var projected = await step.State.Step(projector, (JsonNode)item);
            
            if (projected?.GetValueKind() == JsonValueKind.Object)
            {
                var pObject = projected.AsObject();
                if (!pObject.TryGetPropertyValue("key", out var pKey))
                    continue;
                if (!pObject.TryGetPropertyValue("value", out var pValue))
                    continue;

                switch (pKey?.GetValueKind())
                {
                    case JsonValueKind.Number:
                        res[(int)pKey.GetValue<decimal>()] = pValue?.DeepClone();
                        break;
                    case JsonValueKind.String:
                        res[pKey.GetValue<string>()] = pValue?.DeepClone();
                        break;
                    default:
                        throw new PipeLoomException($"Invalid key type to map. key value is '{pKey?.ToJsonString()}'");
                }
            }
        }

        return res;
    }
}