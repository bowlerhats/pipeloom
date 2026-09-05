using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Relational;

public class JsOpSort: PlOperatorClass
{
    public JsOpSort(IPipeLoomEngine engine)
        : base(engine, "sort")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<JsonNode?>().Mapper(Sort);
        registrator.AsBinary<JsonNode?, string>().Mapper(Sort);
        
        registrator.AsBinary<JsonNode?, Detached<JsonNode?>>().Mapper(Sort);
        registrator.AsTernary<JsonNode?, Detached<JsonNode?>, string>().Mapper(Sort);
    }

    public static JsonNode? Sort(JsonNode? data)
    {
        return Sort(data, "asc");
    }
    
    public static JsonNode? Sort(JsonNode? data, string direction)
    {
        if (data?.GetValueKind() != JsonValueKind.Array)
            return data;
    
        var jsArray = data.AsArray();
        if (jsArray.Count <= 1)
            return jsArray;
        
        var pairs = new List<KeyValuePair<JsonNode?, JsonNode?>>(jsArray.Count);
        pairs.AddRange(jsArray.Select(item => new KeyValuePair<JsonNode?, JsonNode?>(item, item)));
    
        return SortPairs(pairs, direction);
    }
    
    public static ValueTask<JsonNode?> Sort(WeaveStep step, JsonNode? data, Detached<JsonNode?> projector)
    {
        return Sort(step, data, projector, "asc");
    }
    
    public static async ValueTask<JsonNode?> Sort(WeaveStep step, JsonNode? data, Detached<JsonNode?> projector, string direction)
    {
        if (projector.Node.OperatorName == "array")
        {
            // special casing array, since using JsonNode? erased the underlying type,
            // so it is impossible for the engine to fit it properly otherwise
            
            var constValue = await step.State.Step(projector);
            return Sort(constValue, direction);
        }
        
        if (data?.GetValueKind() != JsonValueKind.Array)
            return data;

        var jsArray = data.AsArray();
        if (jsArray.Count <= 1)
            return jsArray;

        var pairs = new List<KeyValuePair<JsonNode?, JsonNode?>>(jsArray.Count); 
        foreach (var item in jsArray)
        {
            if (item is not null)
            {
                var key = await step.State.Step(projector, item);
                pairs.Add(new KeyValuePair<JsonNode?, JsonNode?>(key, item));
            }
            else
            {
                pairs.Add(new KeyValuePair<JsonNode?, JsonNode?>(null, null));
            }
        }

        return SortPairs(pairs, direction);
    }

    public static JsonNode SortPairs(List<KeyValuePair<JsonNode?, JsonNode?>> pairs, string direction)
    {
        var sorter = direction switch
        {
            "asc" => pairs.OrderBy(d => d.Key, SortComparer.Instance),
            "desc" => pairs.OrderByDescending(d => d.Key, SortComparer.Instance),
            _ => throw new PipeLoomException($"Invalid sort direction {direction}")
        };

        var res = new JsonArray();

        foreach (var (_, value) in sorter)
        {
            res.Add(value?.DeepClone());
        }

        return res;
    }


    private sealed class SortComparer : Comparer<JsonNode?>
    {
        public static SortComparer Instance { get; } = new();
        
        public override int Compare(JsonNode? x, JsonNode? y)
        {
            // < 0  ->  x < y

            var xTypeOrder = TypeOrder(x);
            var yTypeOrder = TypeOrder(y);
            if (xTypeOrder != yTypeOrder)
            {
                return xTypeOrder - yTypeOrder;
            }

            return (x?.GetValueKind(), y?.GetValueKind()) switch
            {
                (JsonValueKind.Number, JsonValueKind.Number)
                    => x.GetValue<decimal>().CompareTo(y.GetValue<decimal>()),
                (JsonValueKind.String, JsonValueKind.String)
                    => string.CompareOrdinal(x.GetValue<string>(), y.GetValue<string>()),
                _ => 0
            };
        }

        private static int TypeOrder(JsonNode? node)
        {
            return node?.GetValueKind() switch
            {
                JsonValueKind.False => 1,
                JsonValueKind.True => 2,
                JsonValueKind.Number => 3,
                JsonValueKind.String => 4,
                _ => 5
            };
        }
    }
}