using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.RegexOps;

public class JsOpRegex : PlOperatorClass
{
    public JsOpRegex(IPipeLoomEngine engine)
        : base(engine, "regex")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, string>().Function(RunRegex);
        registrator.AsTernary<JsonNode?, string, string>().Function(RunRegex);
    }

    public static bool RunRegex(JsonNode? value, string pattern)
    {
        if (value?.GetValueKind() != JsonValueKind.String)
            return false;

        var text = value.GetValue<string>();

        return Regex.IsMatch(text, pattern);
    }

    public static bool RunRegex(JsonNode? value, string pattern, string options)
    {
        if (value?.GetValueKind() != JsonValueKind.String)
            return false;

        var text = value.GetValue<string>();

        var opts = ParseJsRegexFlags(options);
        
        return Regex.IsMatch(text, pattern, opts);
    }
    
    public static RegexOptions ParseJsRegexFlags(string? jsFlags)
    {
        var options = RegexOptions.None;

        if (string.IsNullOrEmpty(jsFlags))
            return options;

        foreach (var flag in jsFlags.AsSpan())
        {
            switch (char.ToLowerInvariant(flag))
            {
                case 'i': // ignoreCase
                    options |= RegexOptions.IgnoreCase;
                    break;
                case 'm': // multiline
                    options |= RegexOptions.Multiline;
                    break;
                case 's': // dotAll -> '.' matches newlines
                    options |= RegexOptions.Singleline;
                    break;
            }
        }

        return options;
    }
}