using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Mappers;

public class JsOpJoin : PlOperatorClass
{
    public JsOpJoin(IPipeLoomEngine engine)
        : base(engine, "join")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Mapper(Join);
        registrator.AsBinary<JsonNode?, string>().Mapper(Join);
    }

    public static JsonNode? Join(JsonNode? data)
    {
        return Join(data, string.Empty);
    }
    
    public static JsonNode? Join(JsonNode? data, string? separator)
    {
        if (data?.GetValueKind() != JsonValueKind.Array)
            throw new PipeLoomException("join() expects an array of strings|numbers");

        var jsArray = data.AsArray();

        List<string> parts = [];

        foreach (var item in jsArray)
        {
            switch (item?.GetValueKind())
            {
                case JsonValueKind.String:
                    parts.Add(item.GetValue<string>());
                    break;
                case JsonValueKind.Number:
                    parts.Add(item.GetValue<decimal>().ToString(CultureInfo.InvariantCulture));
                    break;
                default:
                    throw new PipeLoomException("join() expects an array of strings|numbers");
            }
        }
        
        return JsonValue.Create(string.Join(separator, parts));
    }
}