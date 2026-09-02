using System;
using System.Text.Json.Nodes;
using Json.More;
using PipeLoom.JsonQuery.Parsing;
using Shouldly;

namespace PipeLoom.Tests.JsonQueryTests;

public class JsonQueryParserTests
{
    private readonly JsonObject _jsTests;
    
    public JsonQueryParserTests()
    {
        var testsJsonStream = typeof(JsonQueryParserTests).Assembly
                                  .GetManifestResourceStream(typeof(JsonQueryParserTests), "parser.test.json")
                              ?? throw new InvalidOperationException("Parser tests are missing");
        
        _jsTests = JsonNode.Parse(testsJsonStream)?.AsObject()
            ?? throw new InvalidOperationException("Parser tests are missing");
    }

    [Test]
    public void Passes_StdParser_Property()
    {
        this.RunTestsInCategory("property");
    }
    
    [Test]
    public void Passes_StdParser_Functions()
    {
        this.RunTestsInCategory("function");
    }
    
    [Test]
    public void Passes_StdParser_Operators()
    {
        this.RunTestsInCategory("operator");
    }
    
    [Test]
    public void Passes_StdParser_Parenthesis()
    {
        this.RunTestsInCategory("parenthesis");
    }
    
    [Test]
    public void Passes_StdParser_Pipe() {
        this.RunTestsInCategory("pipe");
    }
    
    [Test]
    public void Passes_StdParser_Object() {
        this.RunTestsInCategory("object");
    }
    
    [Test]
    public void Passes_StdParser_Array() {
        this.RunTestsInCategory("array");
    }
    
    [Test]
    public void Passes_StdParser_String() {
        this.RunTestsInCategory("string");
    }
    
    [Test]
    public void Passes_StdParser_Number() {
        this.RunTestsInCategory("number");
    }
    
    [Test]
    public void Passes_StdParser_Boolean() {
        this.RunTestsInCategory("boolean");
    }
    
    [Test]
    public void Passes_StdParser_Null() {
        this.RunTestsInCategory("null");
    }
    
    [Test]
    public void Passes_StdParser_Garbage() {
        this.RunTestsInCategory("garbage");
    }
    
    [Test]
    public void Passes_StdParser_Whitespace() {
        this.RunTestsInCategory("whitespace");
    }
    
    [Test]
    public void Passes_StdParser_Empty() {
        this.RunTestsInCategory("empty");
    }

    [TestCase("{\"a\": 2.5}", """["object",{"a":2.5}]""")]
    [TestCase("{2.5: 2.5}", """["object",{"2.5":2.5}]""")]
    [TestCase("{\"a\": .2.5}", """["object",{"a":["get",2,5]}]""")]
    [TestCase(".\"2\"", """["get", "2"]""")]
    [TestCase(".2", """["get", 2]""")]
    public void ShouldPass_Custom_Parses(string jsq, string expected)
    {
        var parsed = JsonQueryParser.Parse(jsq);
        var jsExpected = JsonNode.Parse(expected);
        
        parsed.IsEquivalentTo(jsExpected).ShouldBe(true,
            $"""
            Parsed: {parsed?.ToJsonString()}
            
            Expected: {jsExpected?.ToJsonString()}
            """
            );
    }

    [TestCase("{2.5_: 2.5}")]
    public void ShouldThrow_Custom_Parses(string jsq)
    {
        Assert.Throws<JsonQueryParseException>(() => JsonQueryParser.Parse(jsq));
    }

    private void RunTestsInCategory(string category)
    {
        var groups = _jsTests["groups"]!.AsArray();
        
        foreach (var group in groups)
        {
            if (group["category"]!.GetValue<string>() != category)
                continue;
            
            var tests = group["tests"]!.AsArray();

            foreach (var test in tests)
            {
                var input = test["input"]?.GetValue<string>();
                input.ShouldNotBeNull();

                var throws = test["throws"];
                if (throws is not null)
                {
                    try
                    {
                        var parsed = JsonQueryParser.Parse(input);

                        Assert.Fail(
                            $"""
                             Parser expected to throw, but was successful.
                             Input: '{input}'
                             Parsed into: '{parsed}'
                             
                             Expected some message close to '{throws}'
                             """
                        );
                    }
                    catch (JsonQueryParseException)
                    {
                        // ignore
                    }
                }
                else
                {
                    var output = test["output"];
                    // output.ShouldNotBeNull();
                    // output.GetValueKind().ShouldBe(JsonValueKind.Array);

                    JsonNode parsedInput = null;
                    try
                    {
                         parsedInput = JsonQueryParser.Parse(input);
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail(
                            $"""
                            Failed to parse input '{input}',
                            {ex.Message}
                            
                            Expected output would've been: '{output?.ToJsonString()}'
                            
                            """
                            );
                    }
                    
                    parsedInput.IsEquivalentTo(output).ShouldBe(true,
                        $"input '{input}' incorrectly parsed into '{parsedInput}', but expected to be '{output}'");

                    
                }
            }
        }
    }
}