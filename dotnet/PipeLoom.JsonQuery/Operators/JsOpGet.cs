using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators;

public class JsOpGet : PlOperatorClass
{
    public JsOpGet(IPipeLoomEngine engine)
        : base(engine, "get")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsVariadic<JsonNode?, JsonNode?>().Mapper(Get);
    }

    private static JsonNode? Get(JsonNode? node, ReadOnlyMemory<JsonNode?> args)
    {
        foreach (var arg in args.Span)
        {
            if (node is null || arg is null)
                return null;

            var argKind = arg.GetValueKind();
            if (argKind is not (JsonValueKind.String or JsonValueKind.Number))
                return null;
            
            var vArg = arg.AsValue();
            
            switch (node.GetValueKind())
            {
                case JsonValueKind.Object:
                    if (!vArg.TryGetValue<string>(out var prop))
                    {
                        if (!vArg.TryGetValue<decimal>(out var pIndex))
                            return null;

                        node = node.AsObject()[(int)pIndex];
                    }
                    else
                    {
                        node = node.AsObject()[prop];
                    }

                    break;
                case JsonValueKind.Array:
                    if (!vArg.TryGetValue<decimal>(out var index))
                        return null;
                    
                    node = node.AsArray()[(int)index];
                    break;
                default:
                    return null;
            }
        }

        return node;
    }
}