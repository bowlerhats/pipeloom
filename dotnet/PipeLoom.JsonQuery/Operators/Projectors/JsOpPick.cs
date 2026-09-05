using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Projectors;

public class JsOpPick: PlOperatorClass
{
    public JsOpPick(IPipeLoomEngine engine)
        : base(engine, "pick")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsVariadic<JsonNode?, Detached<JsonNode?>>().Mapper(Pick);
    }

    public static ValueTask<JsonNode?> Pick(WeaveStep step, JsonNode? data, ReadOnlyMemory<Detached<JsonNode?>> args)
    {
        return data?.GetValueKind() switch
        {
            JsonValueKind.Object => args.Length == 1
                ? PickObjectDirect(step, data, args.Span[0])
                : PickObject(step, data.AsObject(), args),
            JsonValueKind.Array => PickArray(step, data.AsArray(), args),
            _ => throw new PipeLoomException("Pick expects an array of objects or an object")
        };
    }

    public static ValueTask<JsonNode?> PickObjectDirect(WeaveStep step, JsonNode data, Detached<JsonNode?> detached)
    {
        return step.State.Step(detached, data);
    }

    public static async ValueTask<JsonNode?> PickObject(WeaveStep step, JsonObject obj, ReadOnlyMemory<Detached<JsonNode?>> args)
    {
        var res = new JsonObject();

        for (var i = 0; i < args.Length; i++)
        {
            var detached = args.Span[i];
            if (!TryInferPickedPropertyName(detached.Node, out var propName))
                throw new PipeLoomException("Cannot infer property name for pick");

            var projected = await step.State.Step(detached, (JsonNode)obj);
            
            res[propName] = projected?.DeepClone();
        }
        
        return res;
    }
    
    public static async ValueTask<JsonNode?> PickArray(WeaveStep step, JsonArray jsArray, ReadOnlyMemory<Detached<JsonNode?>> args)
    {
        var res = new JsonArray();

        foreach (var item in jsArray)
        {
            if (item?.GetValueKind() != JsonValueKind.Object)
                throw new PipeLoomException("Pick expects element of array to be an object");
            
            res.Add(await PickObject(step, item.AsObject(), args));
        }

        return res;
    }

    public static bool TryInferPickedPropertyName(IWeaveNode node, [MaybeNullWhen(false)] out string propertyName)
    {
        propertyName = null;
        
        if (node.OperatorName != "get")
            return false;

        var lastConstArgument = node.Children
            .LastOrDefault(d => d.IsArgument && d.ImplicitValue != Variant.Undefined);

        if (lastConstArgument is null)
            return false;

        if (lastConstArgument.ImplicitValue.TryUnpack(out string str))
        {
            propertyName = str;
            return true;
        }
        
        if (lastConstArgument.ImplicitValue.TryUnpack(out decimal num))
        {
            propertyName = num.ToString(CultureInfo.InvariantCulture);
            return true;
        }
        
        if (lastConstArgument.ImplicitValue.TryUnpack(out JsonNode? lastNode))
        {
            switch (lastNode?.GetValueKind())
            {
                case JsonValueKind.Number:
                    propertyName = lastNode.GetValue<decimal>().ToString(CultureInfo.InvariantCulture);
                    return true;
                case JsonValueKind.String:
                    propertyName = lastNode.GetValue<string>();
                    return true;
            }
        }

        return false;
    }
}