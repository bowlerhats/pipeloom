using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Relational;

public class JsOpKeyBy : PlOperatorClass
{
    public JsOpKeyBy(IPipeLoomEngine engine)
        : base(engine, "keyBy")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, Detached<JsonNode?>>().Mapper(KeyBy);
    }

    public static async ValueTask<JsonNode?> KeyBy(WeaveStep step, JsonNode? data, Detached<JsonNode?> keyGetter)
    {
        if (data?.GetValueKind() != JsonValueKind.Array)
            throw new PipeLoomException("KeyBy expects an array of objects");
        
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
            
            if (res.ContainsKey(gKey))
                continue;

            res[gKey] = item?.DeepClone();
        }

        return res;
    } 
}