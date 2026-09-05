using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Logical;

public class JsOpNotIn: PlOperatorClass
{
    public JsOpNotIn(IPipeLoomEngine engine)
        : base(engine, "not in")
    {
    }
    
    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, JsonNode?>().Function(NotIn);
    }

    public static bool NotIn(JsonNode? test, JsonNode? values)
    {
        if (values?.GetValueKind() != JsonValueKind.Array)
            throw new PipeLoomException("notin() expects an array of values");

        var jsArray = values.AsArray();
        
        return jsArray.Count == 0 || jsArray.All(item => !JsOpEq.Eq(test, item));
    }
}