using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
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
                        return null;
                    
                    node = node.AsObject()[prop];
                    break;
                case JsonValueKind.Array:
                    if (!vArg.TryGetValue<int>(out var index))
                    {
                        if (!vArg.TryGetValue<string>(out var strIndex))
                            return null;

                        if (!int.TryParse(strIndex, out index))
                            return null;
                    }
                    
                    node = node.AsArray()[index];
                    break;
                default:
                    return null;
            }
        }

        return node;
    }
}