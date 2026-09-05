using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Logical;

public class JsOpExists : PlOperatorClass
{
    public JsOpExists(IPipeLoomEngine engine)
        : base(engine, "exists")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, Detached<JsonNode?>>().Mapper(Exists);
    }

    public static bool Exists(WeaveStep step, JsonNode? data, Detached<JsonNode?> getter)
    {
        if (data?.GetValueKind() != JsonValueKind.Object)
            return false;

        if (getter.Node.OperatorName != "get")
            return false;

        var path = getter.Node.Children
            .Where(static d => d.IsArgument)
            .Select(static d =>
            {
                if (d.ImplicitValue.TryUnpack<string>(out var s))
                    return s;
                
                if (d.ImplicitValue.TryUnpack<JsonNode?>(out var n)
                    && n?.GetValueKind() == JsonValueKind.String)
                {
                    return n.GetValue<string>();
                }

                throw new PipeLoomException("Path segment must be a string");
            })
            .ToArray();

        var jso = data.AsObject();
        
        switch (path.Length)
        {
            case 0: return false;
            case 1: return jso.ContainsKey(path[0]); 
        }
        
        foreach (var prop in path.SkipLast(1))
        {
            if (!jso.ContainsKey(prop))
                return false;

            var next = jso[prop];
            if (next?.GetValueKind() != JsonValueKind.Object)
                return false;

            jso = next.AsObject();
        }

        var lastPath = path.Last();
        return jso.ContainsKey(lastPath);
    }
}