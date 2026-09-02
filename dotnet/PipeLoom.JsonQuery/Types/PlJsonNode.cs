using System.Text.Json.Nodes;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Types.Abstractions;

namespace PipeLoom.JsonQuery.Types;

public class PlJsonNode : PlTypeDef<JsonNode>
{
    public override string Name => "JsonNode";
    public override PlTypeCardinality Cardinality => PlTypeCardinality.Unknown;
    
    public PlJsonNode(IPipeLoomEngine engine) : base(engine)
    {
    }

    protected override void SetupConverters(scoped in ConverterRegistrator convertible)
    {
        base.SetupConverters(in convertible);

        convertible
            .FromValue<decimal>().ToRef<JsonNode>()
            .Using((_, in v) => JsonValue.Create(v));
        
        convertible
            .FromValue<bool>().ToRef<JsonNode>()
            .Using((_, in v) => JsonValue.Create(v));
        
        convertible
            .FromRef<string>().ToRef<JsonNode>()
            .Using((_, v) => JsonValue.Create(v));
    }
}