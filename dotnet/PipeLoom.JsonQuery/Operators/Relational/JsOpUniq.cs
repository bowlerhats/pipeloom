using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.JsonQuery.Operators.Logical;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Relational;

public class JsOpUniq : PlOperatorClass
{
    public JsOpUniq(IPipeLoomEngine engine)
        : base(engine, "uniq")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Mapper(Uniq);
    }

    public static JsonNode? Uniq(JsonNode? data)
    {
        if (data?.GetValueKind() != JsonValueKind.Array)
            throw new PipeLoomException("uniq() expects an array");

        var jsArray = data.AsArray();

        var items = new HashSet<JsonNode?>(JsOpEq.DeepEqualityComparer.Instance);
        foreach (var item in jsArray)
        {
            items.Add(item);
        }
        
        var res = new JsonArray();
        foreach (var item in items)
        {
            res.Add(item?.DeepClone());
        } 

        return res;
    }
}