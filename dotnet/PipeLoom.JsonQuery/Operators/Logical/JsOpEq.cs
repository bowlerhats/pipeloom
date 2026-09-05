using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.More;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions;

namespace PipeLoom.JsonQuery.Operators.Logical;

public class JsOpEq : PlOperatorClass
{
    public JsOpEq(IPipeLoomEngine engine)
        : base(engine, "eq")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsBinary<JsonNode?, JsonNode?>().Function(Eq);
    }

    public static bool Eq(JsonNode? left, JsonNode? right)
    {
        if (left is null && right is null)
            return true;
        
        if (left is null || right is null)
            return false;

        return left.IsEquivalentTo(right);
    }

    public sealed class DeepEqualityComparer : EqualityComparer<JsonNode?>
    {
        public static DeepEqualityComparer Instance { get; } = new();
        
        public override bool Equals(JsonNode? x, JsonNode? y)
        {
            return Eq(x, y);
        }

        public override int GetHashCode(JsonNode obj)
        {
            return obj.GetEquivalenceHashCode();
        }
    } 
}