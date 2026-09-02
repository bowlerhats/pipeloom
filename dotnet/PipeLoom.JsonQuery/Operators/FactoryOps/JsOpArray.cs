using System;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.FactoryOps;

public class JsOpArray : PlOperatorClass
{
    public JsOpArray(IPipeLoomEngine engine)
        : base(engine, "array")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsVariadic<JsonNode?>().Function(ToArray);
    }

    public static JsonNode ToArray(ReadOnlyMemory<JsonNode?> args)
    {
        var res = new JsonArray();
        
        foreach(var arg in args.Span)
        {
            res.Add(arg?.DeepClone());
        }

        return res;
    }
}