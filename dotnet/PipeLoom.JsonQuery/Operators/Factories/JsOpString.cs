using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Factories;

public class JsOpString: PlOperatorClass
{
    public JsOpString(IPipeLoomEngine engine)
        : base(engine, "string")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Function(FormatString);
    }

    public static JsonNode FormatString(JsonNode? node)
    {
        if (node?.GetValueKind() != JsonValueKind.Number)
            throw new PipeLoomException("String formatter expects a number");

        return JsonValue.Create(node.GetValue<decimal>().ToString(CultureInfo.InvariantCulture));
    }
}