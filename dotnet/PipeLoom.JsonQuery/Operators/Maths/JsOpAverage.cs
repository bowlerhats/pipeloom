using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Maths;

public class JsOpAverage : PlOperatorClass
{
    public JsOpAverage(IPipeLoomEngine engine)
        : base(engine, "average")
    {
    }
    
    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Mapper(Average);
    }

    public static JsonNode? Average(JsonNode? data)
    {
        if (data?.GetValueKind() != JsonValueKind.Array)
            throw new PipeLoomException("average() expects an array of numbers");

        var jsArray = data.AsArray();
        if (jsArray.Count == 0)
            throw new PipeLoomException("average() expects an array of numbers");

        var avgSum = JsOpSum.Sum(data);
        var avgCount = JsOpSize.Size(data).GetValue<decimal>();

        return avgCount == 0
            ? throw new PipeLoomException("division by zero")
            : JsonValue.Create(avgSum / avgCount);
    }
}