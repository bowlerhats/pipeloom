using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.JsonQuery.Parsing;

internal static class PlanBuilder
{
    public static void Build(WeaveNode node, JsonArray array)
    {
        if (array.Count <= 0)
            return;

        if (!TryGetString(array[0]!, out var opName))
            return;

        if (string.IsNullOrWhiteSpace(opName))
            throw InvalidQuery();
        
        // special operators
        switch (opName)
        {
            case "object":
                if (BuildObject(node, array))
                    return;
                break;
        }

        var opNode = node.AppendOperator(opName);

        for (var i = 1; i < array.Count; i++)
        {
            var jsNode = array[i];
            if (jsNode is null)
                throw InvalidQuery();
            
            switch (jsNode.GetValueKind())
            {
                case JsonValueKind.Array:
                    Build(opNode, jsNode.AsArray());
                    break;
                case JsonValueKind.String:
                    opNode.AppendValue(jsNode.GetValue<string>());
                    break;
                case JsonValueKind.False:
                    opNode.AppendValue(false);
                    break;
                case JsonValueKind.True:
                    opNode.AppendValue(true);
                    break;
                // case JsonValueKind.Undefined:
                // case JsonValueKind.Null:
                //     opNode.AppendValue(Variant.Undefined);
                //     break;
                case JsonValueKind.Number:
                    opNode.AppendValue(jsNode.GetValue<decimal>());
                    break;
                case JsonValueKind.Object:
                default:
                    throw new PipeLoomException("Unsupported jsonquery AST node");
            }
        }
    }

    private static bool BuildObject(WeaveNode node, JsonArray array)
    {
        if (array.Count != 2 || array[1]?.GetValueKind() != JsonValueKind.Object)
            return false;

        var opNode = node.AppendOperator("object");
        
        var spec = array[1]?.AsObject();
        if (spec is not null)
        {
            foreach(var (key, value) in spec)
            {
                var partOp = opNode.AppendOperator("object");
                partOp.AppendValue(key);
                
                if (value?.GetValueKind() == JsonValueKind.Array)
                {
                    Build(partOp, value.AsArray());
                }
                else
                {
                    partOp.AppendValue(value);
                }
            }
        }

        return true;
    }

    private static bool TryGetString(JsonNode jsNode, [MaybeNullWhen(false)] out string value)
    {
        if (jsNode.GetValueKind() == JsonValueKind.String)
        {
            value = jsNode.GetValue<string>();
            return true;
        }

        value = null;
        return false;
    }

    private static PipeLoomException InvalidQuery()
    {
        return new PipeLoomException("Invalid query");
    }
}