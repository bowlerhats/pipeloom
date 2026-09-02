using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.JsonQuery.Parsing;

namespace PipeLoom.JsonQuery;

public static class JsonQueryUtils
{
    public static JsonNode? Parse(string source)
    {
        return JsonQueryParser.Parse(source);
    }

    public static bool IsTruthy(JsonNode? node)
    {
        if (node is null)
            return false;
        
        switch (node.GetValueKind()) 
        {
            case JsonValueKind.Number:
                return node.GetValue<decimal>() != 0;
            case JsonValueKind.String:
                var s = node.GetValue<string?>();
                return !string.IsNullOrEmpty(s);
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
            case JsonValueKind.False:
                return false;
            
            case JsonValueKind.Array:
                return node.AsArray().Count > 0;
            
            case JsonValueKind.True:
            case JsonValueKind.Object:
                return true;
            default:
                return false;
        }
    }
}