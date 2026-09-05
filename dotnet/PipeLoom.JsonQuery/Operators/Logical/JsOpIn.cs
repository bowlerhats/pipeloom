using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Logical;

public class JsOpIn : PlOperatorClass
{
    public JsOpIn(IPipeLoomEngine engine)
        : base(engine, "in")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, JsonNode?>().Function(In);
    }

    public static bool In(JsonNode? test, JsonNode? values)
    {
        if (values?.GetValueKind() != JsonValueKind.Array)
            throw new PipeLoomException("in() expects an array of values");

        var jsArray = values.AsArray();
        
        return jsArray.Count != 0 && jsArray.Any(item => JsOpEq.Eq(test, item));
    }
}