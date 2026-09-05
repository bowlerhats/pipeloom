using System;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Logical;

public class JsOpOr : PlOperatorClass
{
    public JsOpOr(IPipeLoomEngine engine)
        : base(engine, "or")
    {
        
    }
    
    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);
        
        registrator.AsUnary<JsonNode?>().Function(Or);
        registrator.AsBinary<JsonNode?, JsonNode?>().Function(Or);
        registrator.AsVariadic<JsonNode?>().Function(Or);
    }

    public static bool Or(JsonNode? left)
    {
        throw new PipeLoomException("'or' operator needs at least two arguments");
    }
    
    public static bool Or(JsonNode? left, JsonNode? right)
    {
        return JsonQueryUtils.IsTruthy(left) || JsonQueryUtils.IsTruthy(right);
    }

    public static bool Or(ReadOnlyMemory<JsonNode?> args)
    {
        var res = false;
        
        for (var i = 0; i < args.Length; i++)
        {
            res |= JsonQueryUtils.IsTruthy(args.Span[i]);
            
            if (res)
                break;
        }

        return res;
    }
}