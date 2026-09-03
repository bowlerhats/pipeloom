using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Maths;

public class JsOpAdd : PlOperatorClass
{
    public JsOpAdd(IPipeLoomEngine engine)
        : base(engine, "add")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, JsonNode?>().Function(Add);
    }

    public static JsonNode? Add(JsonNode? left, JsonNode? right)
    {
        if (left is null)
        {
            return right ?? JsonValue.Create((decimal)0);
        }

        if (right is null)
        {
            return left;
        }

        switch (left.GetValueKind(), right.GetValueKind())
        {
            case (JsonValueKind.String, JsonValueKind.String):
                return JsonValue.Create(string.Concat(left.GetValue<string>(), right.GetValue<string>()));
            case (JsonValueKind.Number, JsonValueKind.Number):
                return JsonValue.Create(left.GetValue<decimal>() + right.GetValue<decimal>());
            case (JsonValueKind.Number, JsonValueKind.String):
                return JsonValue.Create(string.Concat(
                    left.GetValue<decimal>().ToString(CultureInfo.InvariantCulture),
                    right.GetValue<string>()));
            case (JsonValueKind.String, JsonValueKind.Number):
                return JsonValue.Create(string.Concat(
                    left.GetValue<string>(),
                    right.GetValue<decimal>().ToString(CultureInfo.InvariantCulture)));
            default:
                throw new PipeLoomException($"Invalid values to add together: left('{left.ToJsonString()}'), right('{right.ToJsonString()}')");
        }
    }
}