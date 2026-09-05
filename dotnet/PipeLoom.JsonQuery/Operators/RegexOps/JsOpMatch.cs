using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.RegexOps;

public class JsOpMatch : PlOperatorClass
{
    public JsOpMatch(IPipeLoomEngine engine)
        : base(engine, "match")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, string?>().Function(Match);
        registrator.AsTernary<JsonNode?, string?, string?>().Function(Match);
    }

    public static JsonNode? Match(JsonNode? data, string? pattern)
    {
        return Match(data, pattern, null);
    }
    
    public static JsonNode? Match(JsonNode? data, string? pattern, string? options)
    {
        if (data?.GetValueKind() != JsonValueKind.String || pattern is null)
            return null;

        var text = data.GetValue<string>();

        var opts = JsOpRegex.ParseJsRegexFlags(options);

        var regex = new Regex(pattern, opts);
        
        var match = regex.Match(text);
        
        return !match.Success ? null : MatchToObject(match);
    }

    public static JsonObject MatchToObject(Match match)
    {
        var res = new JsonObject
        {
            ["value"] = match.Value
        };

        if (match.Groups.Count > 1)
        {
            var groups = new JsonArray();
            var namedGroups = new JsonObject();
            var hasNamedGroup = false;
            
            foreach (Group group in match.Groups)
            {
                if (group.Name == "0")
                    continue;
                
                groups.Add((JsonNode)JsonValue.Create(group.Value));
                
                if (!string.IsNullOrWhiteSpace(group.Name) && !int.TryParse(group.Name, out _))
                {
                    hasNamedGroup = true;
                    namedGroups[group.Name] = JsonValue.Create(group.Value);
                }
            }

            res["groups"] = groups;
            
            if (hasNamedGroup)
                res["namedGroups"] = namedGroups;
        }

        return res;
    }
}