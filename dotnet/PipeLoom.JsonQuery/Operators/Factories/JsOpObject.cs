using System;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Factories;

public class JsOpObject : PlOperatorClass
{
    public JsOpObject(IPipeLoomEngine engine)
        : base(engine, "object")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsNullary().Function(EmptyObject);
        
        registrator.AsVariadic<JsonNode?, JsonNode?>().Mapper(MergeObject);
        registrator.AsBinary<string, JsonNode?>().Function(MakeObject);
    }

    public static JsonNode EmptyObject()
    {
        return new JsonObject();
    }
    
    public static JsonNode MergeObject(JsonNode? data, ReadOnlyMemory<JsonNode?> args)
    {
        var res = new JsonObject();
        foreach (var jsonNode in args.Span)
        {
            if (jsonNode is JsonObject jso)
            {
                foreach (var (key, value) in jso)
                {
                    res[key] = value?.DeepClone();
                }
            }
        }
        
        return res;
    }

    public static JsonNode MakeObject(string prop, JsonNode? value)
    {
        return new JsonObject
        {
            [prop] = value?.DeepClone()
        };
    }
}