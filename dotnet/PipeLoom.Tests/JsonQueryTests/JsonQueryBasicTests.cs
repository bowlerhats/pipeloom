using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using PipeLoom.Builder;
using PipeLoom.Engine;
using PipeLoom.JsonQuery;
using Shouldly;

namespace PipeLoom.Tests.JsonQueryTests;

public class JsonQueryBasicTests : IDisposable
{
    private readonly PipeLoomEngine _engine;
    private readonly JsonNode _data;
    
    public JsonQueryBasicTests()
    {
        _engine = PipeLoomBuilder.Create()
            .AddJsonQuery()
            .Build();

        _data = JsonNode.Parse(
            """
            [ { "name": "John", "age": 31 }, { "name": "Jack", "age": 41 } ]
            """
        );
    }
    
    public void Dispose()
    {
        _engine?.Dispose();
    }
    
    [TestCase(".0.age", "31")]
    [TestCase(".5.age", "null")]
    [TestCase(".0 | get(\"age\")", "31")]
    [TestCase("get(0) | get(\"age\")", "31")]
    [TestCase("get(0) | .age", "31")]
    [TestCase(".0.name", "\"John\"")]
    [TestCase(".0 | .name", "\"John\"")]
    [TestCase("[.0.name]", "[\"John\"]")]
    [TestCase("{ \"a\": .0.name }", "{\"a\":\"John\"}")]
    [TestCase("{ \"a\": .0.name, \"b\": .1.age }", "{\"a\":\"John\",\"b\":41}")]
    [TestCase("{ \"a\": .0.name } | .a", "\"John\"")]
    [TestCase("{ \"a\": .0 | .name } | .a", "\"John\"")]
    [TestCase("{ \"a\": .0 } | .a | .name", "\"John\"")]
    [TestCase("[.0, .1]", """[{"name":"John","age":31},{"name":"Jack","age":41}]""")]
    public async Task Can_Basic_Transform(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }

    [TestCase("filter(.age == 41) | .0.name", "\"Jack\"")]
    [TestCase("filter(.age == 41 and .name == \"Jack\") | .0.name", "\"Jack\"")]
    [TestCase("filter(.age == 141)", "[]")]
    [TestCase("filter(.age == 141) | .0.name", "null")]
    public async Task Can_Basic_Filter(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }

    [TestCase("[1, 2] | sum()", "3")]
    [TestCase("sum([1, 2])", "3")]
    [TestCase("sum([])", "0")]
    [TestCase("[] | sum()", "0")]
    public async Task Can_Sum(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("map(.age)", "[31,41]")]
    [TestCase("map({ age2: .age})", """[{"age2":31},{"age2":41}]""")]
    [TestCase("map(12)", "[12,12]")]
    [TestCase("map(if(.age == 31, .name, .age))", "[\"John\",41]")]
    public async Task Can_Map(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("filter(regex(.name, \"ack\")) | map(.age)", "[41]")]
    [TestCase("filter(regex(.name, \"Ack\")) | map(.age)", "[]")]
    [TestCase("filter(regex(.name, \"Ack\", \"i\")) | map(.age)", "[41]")]
    [TestCase("filter(regex(.name, \"ac?k\")) | map(.age)", "[41]")]
    public async Task Can_Regex(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }

    private async Task Run(string jsq, string expected)
    {
        using var plan = new WeavePlan(_engine);
        plan.AppendJsonQuery(jsq);
        await plan.WithInputCarry<JsonNode>().Fuse<JsonNode>();

        var execResult = await _engine.Execute<JsonNode, JsonNode>(plan, _data);
        var res = execResult?.ToJsonString() ?? "null";
        
        res.ShouldBe(expected);
    }
}