using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Mappers;

public class JsOpSubstring : PlOperatorClass
{
    public JsOpSubstring(IPipeLoomEngine engine)
        : base(engine, "substring")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, decimal>().Function(Substring);
        registrator.AsTernary<JsonNode?, decimal, decimal>().Function(Substring);
    }

    public static JsonNode Substring(JsonNode? data, decimal start)
    {
        if (data?.GetValueKind() != JsonValueKind.String)
            throw new PipeLoomException("substring() expects a string");

        var str = data.GetValue<string>();
        var pStart = Math.Max((int)start, 0);

        return str[pStart..];
    }
    
    public static JsonNode Substring(JsonNode? data, decimal start, decimal end)
    {
        if (data?.GetValueKind() != JsonValueKind.String)
            throw new PipeLoomException("substring() expects a string");

        var str = data.GetValue<string>();
        
        var pStart = Math.Max((int)start, 0);
        var length = (int)(end - pStart);
        
        return str.Substring(pStart, length);
    }
}