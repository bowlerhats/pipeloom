using System;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Logical;

public class JsOpAnd : PlOperatorClass
{
    public JsOpAnd(IPipeLoomEngine engine)
        : base(engine, "and")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);
        
        registrator.AsUnary<JsonNode?>().Function(And);
        registrator.AsBinary<JsonNode?, JsonNode?>().Function(And);
        registrator.AsVariadic<JsonNode?>().Function(And);
    }

    public static bool And(JsonNode? left)
    {
        throw new PipeLoomException("'and' operator needs at least two arguments");
    }
    
    public static bool And(JsonNode? left, JsonNode? right)
    {
        return JsonQueryUtils.IsTruthy(left) && JsonQueryUtils.IsTruthy(right);
    }

    public static bool And(ReadOnlyMemory<JsonNode?> args)
    {
        var res = args.Length > 1;
        if (!res)
            return false;
        
        for (var i = 0; i < args.Length; i++)
        {
            res &= JsonQueryUtils.IsTruthy(args.Span[i]);
            
            if (!res)
                break;
        }

        return res;
    }
    
}