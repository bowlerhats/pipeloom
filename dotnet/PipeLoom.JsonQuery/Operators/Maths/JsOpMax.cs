using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Maths;

public class JsOpMax : PlOperatorClass
{
    public JsOpMax(IPipeLoomEngine engine)
        : base(engine, "max")
    {
    }
    
    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Mapper(Max);
    }

    public static JsonNode? Max(JsonNode? data)
    {
        if (data?.GetValueKind() != JsonValueKind.Array)
            throw new PipeLoomException("max() expectes an array of numbers");

        var jsArray = data.AsArray();
        if (jsArray.Count == 0)
            return null;

        var max = decimal.MinValue;
        
        foreach (var item in jsArray)
        {
            if (item?.GetValueKind() != JsonValueKind.Number)
                throw new PipeLoomException("max() expectes an array of numbers");

            var value = item.GetValue<decimal>();
            if (value > max)
            {
                max = value;
            }
        }
        
        return JsonValue.Create(max);
    }
}