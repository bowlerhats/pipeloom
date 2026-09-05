using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Projectors;

public class JsOpGet : PlOperatorClass
{
    public JsOpGet(IPipeLoomEngine engine)
        : base(engine, "get")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsNullary().Function(Get);
        registrator.AsVariadic<JsonNode?, JsonNode?>().Mapper(Get);
    }

    public static JsonNode? Get(WeaveStep step)
    {
        return step.State.Carry.TryUnpack<JsonNode?>(out var carry)
            ? carry : null;
    }

    public static JsonNode? Get(JsonNode? data, ReadOnlyMemory<JsonNode?> args)
    {
        foreach (var arg in args.Span)
        {
            if (data is null || arg is null)
                return null;

            var argKind = arg.GetValueKind();
            if (argKind is not (JsonValueKind.String or JsonValueKind.Number))
                return null;
            
            var vArg = arg.AsValue();
            
            switch (data.GetValueKind())
            {
                case JsonValueKind.Object:
                    if (!vArg.TryGetValue<string>(out var prop))
                    {
                        if (!vArg.TryGetValue<decimal>(out var pIndex))
                            return null;

                        data = data.AsObject()[(int)pIndex];
                    }
                    else
                    {
                        data = data.AsObject()[prop];
                    }

                    break;
                case JsonValueKind.Array:
                    if (!vArg.TryGetValue<decimal>(out var index))
                    {
                        if (!vArg.TryGetValue<string>(out var sIndex))
                            throw new PipeLoomException("Expected number or number-as-string as an array index");
                        
                        if (!decimal.TryParse(sIndex, CultureInfo.InvariantCulture, out index))
                            throw new PipeLoomException($"Invalid array indexer, cannot be parsed to a number: '{sIndex}'");
                    }
                    
                    if (decimal.Truncate(index) != index)
                        throw new PipeLoomException($"Array indexer should be an integer, but got fractional: '{index}'");

                    var idx = (int)index;
                    var asArray = data.AsArray();
                    if (idx < 0 || idx >= asArray.Count)
                        return null;
                    
                    data = asArray[idx];
                    break;
                default:
                    return null;
            }
        }

        return data;
    }
}