using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Factories;

public class JsOpNumber : PlOperatorClass
{
    public JsOpNumber(IPipeLoomEngine engine)
        : base(engine, "number")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<string>().Function(Number);
        registrator.AsUnary<JsonNode?>().Function(Number);
    }

    public static JsonNode Number(string str)
    {
        return JsonValue.Create(decimal.Parse(str, CultureInfo.InvariantCulture));
    }
    
    public static JsonNode Number(JsonNode? node)
    {
        return node?.GetValueKind() switch
        {
            JsonValueKind.Number => node,
            JsonValueKind.String => decimal.Parse(node.GetValue<string>(), CultureInfo.InvariantCulture),
            _ => throw new PipeLoomException("Parsing a number requires a text or a ready made value")
        };
    }
}