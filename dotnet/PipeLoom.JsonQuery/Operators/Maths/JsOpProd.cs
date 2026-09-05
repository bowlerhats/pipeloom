using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Maths;

public class JsOpProd : PlOperatorClass
{
    public JsOpProd(IPipeLoomEngine engine)
        : base(engine, "prod")
    {
    }
    
    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Mapper(Prod);
    }

    public static JsonNode? Prod(JsonNode? data)
    {
        if (data?.GetValueKind() != JsonValueKind.Array)
            throw new PipeLoomException("prod() expectes an array of numbers");

        var jsArray = data.AsArray();
        if (jsArray.Count == 0)
            return null;

        var prod = 1M;
        
        foreach (var item in jsArray)
        {
            if (item?.GetValueKind() != JsonValueKind.Number)
                throw new PipeLoomException("prod() expectes an array of numbers");

            prod *= item.GetValue<decimal>();
            if (prod == 0)
                break;
        }
        
        return JsonValue.Create(prod);
    }
}