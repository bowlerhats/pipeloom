using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.RegexOps;

public class JsOpMatchAll : PlOperatorClass
{
    public JsOpMatchAll(IPipeLoomEngine engine)
        : base(engine, "matchAll")
    {
    }
    
    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, string?>().Function(MatchAll);
        registrator.AsTernary<JsonNode?, string?, string?>().Function(MatchAll);
    }

    public static JsonNode? MatchAll(JsonNode? data, string? pattern)
    {
        return MatchAll(data, pattern, null);
    }
    
    public static JsonNode? MatchAll(JsonNode? data, string? pattern, string? options)
    {
        if (data?.GetValueKind() != JsonValueKind.String || pattern is null)
            return null;

        var text = data.GetValue<string>();

        var opts = JsOpRegex.ParseJsRegexFlags(options);

        var regex = new Regex(pattern, opts);

        var res = new JsonArray();
        
        var matches = regex.Matches(text);

        foreach (Match match in matches)
        {
            if (match.Success)
            {
                res.Add((JsonNode)JsOpMatch.MatchToObject(match));
            }
        }
        
        return res;
    }
}