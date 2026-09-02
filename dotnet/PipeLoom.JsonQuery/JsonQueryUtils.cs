using System.Text.Json.Nodes;
using PipeLoom.JsonQuery.Parsing;

namespace PipeLoom.JsonQuery;

public static class JsonQueryUtils
{
    public static JsonNode? Parse(string source)
    {
        return JsonQueryParser.Parse(source);
    }
}