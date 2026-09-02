// JsonNodeSchemaInferrer.cs
//
// Infers a JsonSchema.Net JsonSchema from one or more System.Text.Json.Nodes.JsonNode
// instances (i.e. from actual JSON data, not from a .NET type).
//
// AOT-safe by construction: every path here is pattern-matching over JsonValueKind /
// JsonObject / JsonArray / JsonValue. No reflection, no JsonSerializer calls, no
// generic GetValue<T>() beyond the two hand-written fast paths STJ ships for JsonElement
// and long/int, which do not go through the serializer.
//
// NuGet deps: JsonSchema.Net (json-everything). System.Text.Json is part of the BCL.
//
// Known simplifications (documented so nobody mistakes this for a spec-complete tool):
//  - number vs integer is decided per literal (5 -> integer, 5.0 -> number); a field
//    that's sometimes whole and sometimes not is reported as [integer, number]
//  - "required" is the intersection of object keys seen across every sample that hit
//    a given schema location; a key missing from even one sample becomes optional
//  - arrays are homogeneous-tuple-free: every element folds into one unified item
//    schema, there's no positional/tuple validation
//  - string formats (date-time, uuid, email, ...) are not guessed
//  - no $ref/$defs extraction - repeated object shapes are inlined every time they occur
//
// Example usage:
//
//   var doc = JsonNode.Parse(File.ReadAllText("sample.json"));
//   JsonSchema schema = JsonNodeSchemaInferrer.Infer(doc);
//
//   // or, better: infer from several examples so "required" reflects reality
//   var samples = files.Select(f => JsonNode.Parse(File.ReadAllText(f)));
//   JsonSchema schema = JsonNodeSchemaInferrer.InferMany(samples);
//
//   Console.WriteLine(JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true }));

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;

namespace PipeLoom.JsonQuery.Parsing;

public static class JsonNodeSchemaInferrer
{
    /// <summary>
    /// Infers a schema from a single JSON instance.
    /// </summary>
    /// <param name="node">The parsed JSON document (or null for JSON `null`).</param>
    /// <param name="allowAdditionalProperties">
    /// Whether inferred object schemas permit keys not seen in the sample(s).
    /// Default is false (closed objects) since that's usually what you want when
    /// generating a schema from example data - flip to true for validation-only use
    /// where unknown extra fields should be tolerated.
    /// </param>
    public static JsonSchema Infer(JsonNode? node, bool allowAdditionalProperties = false)
        => WalkNode(node).ToBuilder(allowAdditionalProperties).Build();

    /// <summary>
    /// Infers a schema by unioning several example instances. Prefer this over
    /// <see cref="Infer"/> when you have more than one sample: it's what gives you an
    /// accurate "required" list and lets optional/nullable fields surface, instead of
    /// every field in a single sample looking mandatory.
    /// </summary>
    public static JsonSchema InferMany(IEnumerable<JsonNode?> samples, bool allowAdditionalProperties = false)
    {
        NodeShape? merged = null;
        foreach (var sample in samples)
        {
            var shape = WalkNode(sample);
            merged = merged is null ? shape : merged.MergeWith(shape);
        }

        return (merged ?? NodeShape.Empty).ToBuilder(allowAdditionalProperties).Build();
    }

    // ---- walking ---------------------------------------------------------------

    private static NodeShape WalkNode(JsonNode? node)
    {
        if (node is null)
            return new NodeShape { SeenNull = true };

        switch (node)
        {
            case JsonObject obj:
            {
                var shape = new NodeShape { SeenObject = true };
                foreach (var (key, value) in obj)
                    shape.Properties[key] = WalkNode(value);
                shape.RequiredIntersection = new HashSet<string>(obj.Select(kv => kv.Key));
                return shape;
            }

            case JsonArray arr:
            {
                var shape = new NodeShape { SeenArray = true };
                foreach (var element in arr)
                {
                    var elementShape = WalkNode(element);
                    shape.ItemShape = shape.ItemShape is null
                        ? elementShape
                        : shape.ItemShape.MergeWith(elementShape);
                }
                return shape;
            }

            case JsonValue value:
                return WalkValue(value);

            default:
                // Unreachable in practice - JsonNode only has the three derived types
                // above - but keep the switch exhaustive-safe rather than throwing.
                return NodeShape.Empty;
        }
    }

    private static NodeShape WalkValue(JsonValue value)
    {
        // GetValueKind() reads the stored primitive / JsonElement's kind directly; it
        // does not invoke JsonSerializer, so this is AOT-safe.
        var kind = value.GetValueKind();
        return kind switch
        {
            JsonValueKind.String => new NodeShape { SeenString = true },
            JsonValueKind.True or JsonValueKind.False => new NodeShape { SeenBool = true },
            JsonValueKind.Null => new NodeShape { SeenNull = true },
            JsonValueKind.Number => IsIntegerValued(value)
                ? new NodeShape { SeenInteger = true }
                : new NodeShape { SeenNumber = true },
            _ => NodeShape.Empty,
        };
    }

    private static bool IsIntegerValued(JsonValue value)
    {
        // Fast, AOT-safe path: JsonValue.TryGetValue<JsonElement> is a hand-written
        // accessor for JsonElement-backed values (the common case - anything that came
        // from JsonNode.Parse) and never touches JsonSerializer.
        if (value.TryGetValue(out JsonElement element))
        {
            var raw = element.GetRawText();
            return raw.IndexOf('.') < 0 && raw.IndexOf('e') < 0 && raw.IndexOf('E') < 0;
        }

        // Fallback for values built in-memory via JsonValue.Create(someNumber) rather
        // than parsed from text. TryGetValue<T> for these built-in numeric T is also a
        // dedicated fast path in STJ, not a reflective/serializer one.
        return value.TryGetValue(out long _)
            || value.TryGetValue(out int _)
            || value.TryGetValue(out short _)
            || value.TryGetValue(out byte _);
    }

    // ---- accumulator -------------------------------------------------------------
    //
    // Walking builds this small intermediate model instead of a JsonSchemaBuilder
    // directly, because merging two shapes (array elements, or two top-level samples
    // in InferMany) is straightforward here and awkward against JsonSchemaBuilder /
    // JsonSchema directly.

    private sealed class NodeShape
    {
        public static NodeShape Empty => new();

        public bool SeenNull;
        public bool SeenBool;
        public bool SeenInteger;
        public bool SeenNumber; // non-integer number literal (has '.' or exponent)
        public bool SeenString;

        public bool SeenArray;
        public NodeShape? ItemShape;

        public bool SeenObject;
        public Dictionary<string, NodeShape> Properties { get; } = new();

        // null = "no object sample folded in yet"; distinct from "seen an object with
        // zero properties", which is HashSet<string>() (empty but non-null).
        public HashSet<string>? RequiredIntersection;

        public NodeShape MergeWith(NodeShape other)
        {
            var result = new NodeShape
            {
                SeenNull = SeenNull || other.SeenNull,
                SeenBool = SeenBool || other.SeenBool,
                SeenInteger = SeenInteger || other.SeenInteger,
                SeenNumber = SeenNumber || other.SeenNumber,
                SeenString = SeenString || other.SeenString,
                SeenArray = SeenArray || other.SeenArray,
                SeenObject = SeenObject || other.SeenObject,
            };

            if (result.SeenArray)
            {
                result.ItemShape = ItemShape is null ? other.ItemShape
                    : other.ItemShape is null ? ItemShape
                    : ItemShape.MergeWith(other.ItemShape);
            }

            if (result.SeenObject)
            {
                foreach (var key in this.Properties.Keys.Union(other.Properties.Keys))
                {
                    var a = this.Properties.GetValueOrDefault(key);
                    var b = other.Properties.GetValueOrDefault(key);
                    result.Properties[key] = a is null ? b! : b is null ? a : a.MergeWith(b);
                }

                result.RequiredIntersection = (RequiredIntersection, other.RequiredIntersection) switch
                {
                    (null, var r) => r,
                    (var l, null) => l,
                    (var l, var r) => new HashSet<string>(l!.Intersect(r!)),
                };
            }

            return result;
        }

        public JsonSchemaBuilder ToBuilder(bool allowAdditionalProperties)
        {
            var builder = new JsonSchemaBuilder();

            var typeList = new List<SchemaValueType>(capacity: 6);
            if (SeenObject) typeList.Add(SchemaValueType.Object);
            if (SeenArray) typeList.Add(SchemaValueType.Array);
            if (SeenString) typeList.Add(SchemaValueType.String);
            // JSON Schema's "number" already covers whole numbers; we only add the
            // narrower "integer" when every numeric sample we saw was whole, and add
            // both if we saw a mix (2 one sample, 2.5 another).
            if (SeenInteger) typeList.Add(SchemaValueType.Integer);
            if (SeenNumber) typeList.Add(SchemaValueType.Number);
            if (SeenBool) typeList.Add(SchemaValueType.Boolean);
            if (SeenNull) typeList.Add(SchemaValueType.Null);

            switch (typeList.Count)
            {
                case 0:
                    // Never observed a value at this location (e.g. InferMany with no
                    // samples). Leave the schema without a "type" constraint - matches
                    // any instance, which is the honest answer given zero evidence.
                    break;
                case 1:
                    builder = builder.Type(typeList[0]);
                    break;
                default:
                    builder = builder.Type(typeList.ToArray());
                    break;
            }

            if (SeenObject)
            {
                if (this.Properties.Count > 0)
                {
                    var propertyTuples = this.Properties
                        .Select(kv => (kv.Key, kv.Value.ToBuilder(allowAdditionalProperties)))
                        .ToArray();
                    builder = builder.Properties(propertyTuples);
                }

                var required = (RequiredIntersection ?? new HashSet<string>(this.Properties.Keys)).ToArray();
                if (required.Length > 0)
                    builder = builder.Required(required);

                builder = builder.AdditionalProperties(allowAdditionalProperties);
            }

            if (SeenArray && ItemShape is not null)
                builder = builder.Items(ItemShape.ToBuilder(allowAdditionalProperties));

            return builder;
        }
    }
}