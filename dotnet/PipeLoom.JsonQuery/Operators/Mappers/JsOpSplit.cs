using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Mappers;

public class JsOpSplit : PlOperatorClass
{
    
    public JsOpSplit(IPipeLoomEngine engine)
        : base(engine, "split")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Mapper(SplitByWords);
        registrator.AsBinary<JsonNode?, string>().Mapper(SplitBySeparator);
    }

    public static JsonNode SplitByWords(JsonNode? data)
    {
        if (data?.GetValueKind() != JsonValueKind.String)
            throw new PipeLoomException("split() expects a string");

        var str = data.GetValue<string>();
        if (string.IsNullOrWhiteSpace(str))
            return new JsonArray();
        
        var res = new JsonArray();
        var strSpan = str.AsSpan();

        foreach (var range in strSpan.SplitAny(ReadOnlySpan<char>.Empty))
        {
            var s = strSpan[range].Trim();
            if (!s.IsEmpty)
                res.Add((JsonNode)JsonValue.Create(s.ToString()));
        }

        return res;
    }

    public static JsonNode SplitBySeparator(JsonNode? data, string separator)
    {
        if (data?.GetValueKind() != JsonValueKind.String)
            throw new PipeLoomException("split() expects a string");

        var str = data.GetValue<string>();
        if (string.IsNullOrEmpty(str))
            return new JsonArray();

        if (string.IsNullOrEmpty(separator))
            return SplitIntoChars(str);

        var res = new JsonArray();
        
        var strSpan = str.AsSpan();
        foreach (var range in strSpan.Split(separator))
        {
            var s = strSpan[range];
            
            res.Add((JsonNode)JsonValue.Create(s.ToString()));
        }

        return res;
    }

    public static JsonNode SplitIntoChars(string data)
    {
        var res = new JsonArray();

        foreach (var c in data.AsSpan())
        {
            res.Add((JsonNode)JsonValue.Create(c.ToString()));
        }

        return res;
    }
}