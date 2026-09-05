using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Maths;

public class JsOpMin : PlOperatorClass
{
    public JsOpMin(IPipeLoomEngine engine)
        : base(engine, "min")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Mapper(Min);
    }

    public static JsonNode? Min(JsonNode? data)
    {
        if (data?.GetValueKind() != JsonValueKind.Array)
            throw new PipeLoomException("min() expectes an array of numbers");

        var jsArray = data.AsArray();
        if (jsArray.Count == 0)
            return null;

        var min = decimal.MaxValue;
        
        foreach (var item in jsArray)
        {
            if (item?.GetValueKind() != JsonValueKind.Number)
                throw new PipeLoomException("min() expectes an array of numbers");

            var value = item.GetValue<decimal>();
            if (value < min)
            {
                min = value;
            }
        }
        
        return JsonValue.Create(min);
    }
}