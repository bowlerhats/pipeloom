using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Maths;

public class JsOpSum: PlOperatorClass
{
    public JsOpSum(IPipeLoomEngine engine)
        : base(engine, "sum")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Mapper(Sum);
    }

    private static decimal Sum(JsonNode? node)
    {
        if (node?.GetValueKind() != JsonValueKind.Array)
            return 0;
        
        var jsArray = node.AsArray();
        if (jsArray.Count == 0)
            return 0;

        return jsArray.Sum(static v => v?.GetValue<decimal>() ?? 0);
    }
}