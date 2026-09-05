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
            [
              { "name": "John", "age": 31, "address": { "city": "New York" } },
              { "name": "Jack", "age": 41, "address": { "city": "Washington" } }
            ]
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
    [TestCase("[.0, .1]", """[{"name":"John","age":31,"address":{"city":"New York"}},{"name":"Jack","age":41,"address":{"city":"Washington"}}]""")]
    [TestCase("get()", """[{"name":"John","age":31,"address":{"city":"New York"}},{"name":"Jack","age":41,"address":{"city":"Washington"}}]""")]
    public async Task Can_Basic_Transform(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }

    [TestCase("filter(.age == 41) | .0.name", "\"Jack\"")]
    [TestCase("filter(.age == 41 and .name == \"Jack\") | .0.name", "\"Jack\"")]
    [TestCase("filter(.age == 141)", "[]")]
    [TestCase("filter(.age == 141) | .0.name", "null")]
    public async Task Can_Filter(string jsq, string expected)
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
    
    [TestCase(""" match("abc", "b") """, """{"value":"b"}""")]
    [TestCase(""" match("Save the date: 2025-12-25!", "(?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<date>[0-9]{2})") """,
        """{"value":"2025-12-25","groups":["2025","12","25"],"namedGroups":{"year":"2025","month":"12","date":"25"}}""")]
    [TestCase(""" match("abc", "(bc)$") """, """{"value":"bc","groups":["bc"]}""")]
    [TestCase(""" match("abc", "(bc)") """, """{"value":"bc","groups":["bc"]}""")]
    [TestCase(""" match("abc", "(?<efg>bc)$") """, """{"value":"bc","groups":["bc"],"namedGroups":{"efg":"bc"}}""")]
    public async Task Can_Match(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(""" matchAll("abcbd", "b") """, """[{"value":"b"},{"value":"b"}]""")]
    [TestCase(""" matchAll("abcbdb", "b.") """, """[{"value":"bc"},{"value":"bd"}]""")]
    [TestCase(""" matchAll("abcbdb", "b.?") """, """[{"value":"bc"},{"value":"bd"},{"value":"b"}]""")]
    public async Task Can_MatchAll(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("[2, 1, 3] | sort()", "[1,2,3]")]
    [TestCase("[2, 1, 3] | sort(get(), \"desc\")", "[3,2,1]")]
    [TestCase("sort([2, 1, 3])", "[1,2,3]")]
    [TestCase("map(.age) | sort()", "[31,41]")]
    [TestCase("sort(.age) | map(.name)", "[\"John\",\"Jack\"]")]
    [TestCase("sort(.age, \"desc\") | map(.name)", "[\"Jack\",\"John\"]")]
    [TestCase("sort(.age) | map(.name) | sort()", "[\"Jack\",\"John\"]")]
    [TestCase("map(.age) | sort(\"desc\")", "[41,31]")]
    public async Task Can_Sort(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("[1,2,3] | reverse()", "[3,2,1]")]
    [TestCase("reverse([1,2,3])", "[3,2,1]")]
    [TestCase("map(.age) | reverse()", "[41,31]")]
    [TestCase("reverse(map(.age))", "[41,31]")]
    public async Task Can_Reverse(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("pick(.age)", "[{\"age\":31},{\"age\":41}]")]
    [TestCase("pick(.age, .name)", "[{\"age\":31,\"name\":\"John\"},{\"age\":41,\"name\":\"Jack\"}]")]
    [TestCase("{ \"price\": 2.5 } | pick(.price)", "2.5", Description = "Function reference shows example of standalone pick to be equivalent to a get")]
    public async Task Can_Pick(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(".0 | mapObject({key: (.key), value: (.value)})", """{"name":"John","age":31,"address":{"city":"New York"}}""")]
    [TestCase(""".0 | mapObject({key: (if(.key == "name", "a1", .key)), value: (.value)})""", """{"a1":"John","age":31,"address":{"city":"New York"}}""")]
    [TestCase(".0 | mapObject({key: \"#\" + .key, value: 1})", """{"#name":1,"#age":1,"#address":1}""")]
    public async Task Can_MapObject(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("1+2", "3")]
    [TestCase("1 + 2 + 1.4", "4.4")]
    [TestCase(""" "#"+2""", "\"#2\"")]
    [TestCase(""" 1 + "#" """, "\"1#\"")]
    [TestCase(""" "A" + "B" """, "\"AB\"")]
    [TestCase(""" "A" + "B" + "C" """, "\"ABC\"")]
    [TestCase(""" "A" + "B" + "C" + 123 """, "\"ABC123\"")]
    [TestCase(""" "A" + "B" + "C" + 1e2 """, "\"ABC100\"")]
    [TestCase("21 + 1e2", "121")]
    [TestCase(".0.age + .1.age", "72")]
    [TestCase("21 + -1e2", "-79")]
    public async Task Can_Add(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("3-1", "2")]
    [TestCase("3-10", "-7")]
    [TestCase("3 - -10", "13")]
    [TestCase(".0.age - .1.age", "-10")]
    public async Task Can_Subtract(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("3 * 4", "12")]
    [TestCase("3 * -4", "-12")]
    [TestCase("-1 * 4", "-4")]
    [TestCase(".0.age * .1.age", "1271")]
    [TestCase("5 + .0.age * .1.age", "1276")]
    public async Task Can_Multiply(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("4/2", "2")]
    [TestCase("4/-2", "-2")]
    [TestCase("5 + 4 / -2", "3")]
    [TestCase("(.0.age - 1) / 3", "10")]
    public async Task Can_Divide(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("2^3", "8")]
    [TestCase("2^-1", "0.5")]
    [TestCase("3^0", "1")]
    [TestCase("2 + 2^3", "10")]
    [TestCase("2^3 + 2", "10")]
    [TestCase(".0.age ^ 2", "961")]
    public async Task Can_Pow(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("3%3", "0")]
    [TestCase("3%2", "1")]
    [TestCase("3%-2", "1")]
    public async Task Can_Mod(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("abs(-3)", "3")]
    [TestCase("abs(3)", "3")]
    [TestCase("abs(-1 * .0.age)", "31")]
    public async Task Can_Abs(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("round(23.7612)", "24")]
    [TestCase("round(23.1345, 2)", "23.13")]
    [TestCase("round(23.1365, 2)", "23.14")]
    public async Task Can_Round(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("number(3)", "3")] 
    [TestCase("number(\"3\")", "3")]
    [TestCase("number(-4e3) | number(get())", "-4000")]
    [TestCase(".0.age | number(get())", "31")]
    public async Task Can_Number(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("string(3)", "\"3\"")]
    [TestCase("string(.0.age)", "\"31\"")]
    [TestCase("string(3.334)", "\"3.334\"")]
    public async Task Can_String(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(".0 | mapKeys(\"#\" + get())", """{"#name":"John","#age":31,"#address":{"city":"New York"}}""")]
    public async Task Can_MapKeys(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(".0.address | mapValues(\"#\" + get())", """{"city":"#New York"}""")]
    [TestCase(".0.address | mapValues(2)", """{"city":2}""")]
    public async Task Can_MapValues(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("groupBy(.address.city)", """{"New York":[{"name":"John","age":31,"address":{"city":"New York"}}],"Washington":[{"name":"Jack","age":41,"address":{"city":"Washington"}}]}""")]
    public async Task Can_GroupBy(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("""map({"name": .name}) | keyBy(.name)""", """{"John":{"name":"John"},"Jack":{"name":"Jack"}}""")]
    public async Task Can_KeyBy(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(".0 | keys()", """["name","age","address"]""")]
    public async Task Can_Keys(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(".0 | values()", """["John",31,{"city":"New York"}]""")]
    public async Task Can_Values(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("[[1, 2],[3, 4]] | flatten()", "[1,2,3,4]")]
    [TestCase("[[1, 2, [3, 4]]] | flatten()", "[1,2,[3,4]]")]
    public async Task Can_Flatten(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase("map(.name) | join(\", \")", "\"John, Jack\"")]
    [TestCase("map(.age) | join(\", \")", "\"31, 41\"")]
    [TestCase("map(.name) | join()", "\"JohnJack\"")]
    [TestCase("join(map(.name))", "\"JohnJack\"")]
    [TestCase("join(map(.name), \", \")", "\"John, Jack\"")]
    public async Task Can_Join(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(""" "abc" | split() """, """["abc"]""")]
    [TestCase(""" "a bc" | split() """, """["a","bc"]""")]
    [TestCase(""" "a bc " | split() """, """["a","bc"]""")]
    [TestCase(""" "a,bc" | split(",") """, """["a","bc"]""")]
    [TestCase(""" "abc" | split("") """, """["a","b","c"]""")]
    [TestCase("\"a\tbc\" | split()", """["a","bc"]""")]
    public async Task Can_Split(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(""" substring("2024-11-06 23:14:00", 0, 10) """, "\"2024-11-06\"")]
    [TestCase(""" substring("John", 1) """, "\"ohn\"")]
    public async Task Can_Substring(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" uniq([1,2,3,2,4,4,5]) ", "[1,2,3,4,5]")]
    public async Task Can_Uniq(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(""" [{ "a": 1 }, { "a": 1 }] | uniqBy(.a) """, "[{\"a\":1}]")]
    [TestCase(" uniqBy(.age) | map(.age) | sort() ", "[31,41]")]
    public async Task Can_UniqBy(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" [1,2,3,4,5] | limit(2) ", "[1,2]")]
    [TestCase(" [1,2,3,4,5] | limit(0) ", "[]")]
    [TestCase(" [] | limit(0) ", "[]")]
    [TestCase(" [] | limit(2) ", "[]")]
    public async Task Can_Limit(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" [] | size() ", "0")]
    [TestCase(" [1,2] | size() ", "2")]
    [TestCase(""" size("abc") """, "3")]
    [TestCase(""" size("") """, "0")]
    [TestCase(" size(.0.name) ", "4")]
    [TestCase(" .0.name | size() ", "4")]
    public async Task Can_Size(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" [1,2] | min() ", "1")]
    [TestCase(" [-3,1,2] | min() ", "-3")]
    [TestCase(" [] | min() ", "null")]
    public async Task Can_Min(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" [1,2] | max() ", "2")]
    [TestCase(" [-3,1,2] | max() ", "2")]
    [TestCase(" [] | max() ", "null")]
    public async Task Can_Max(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" [-3,1,2] | prod() ", "-6")]
    public async Task Can_Prod(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" [-3,1,2] | average() ", "0")]
    [TestCase(" [1,2,3] | average() ", "2")]
    public async Task Can_Average(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" 1 == 1 ", "true")]
    [TestCase(" 1 == {} ", "false")]
    [TestCase(" \"abc\" == \"abc\" ", "true")]
    public async Task Can_Eq(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" 1 != 1 ", "false")]
    [TestCase(" 1 != 2 ", "true")]
    [TestCase(" 1 != {} ", "true")]
    [TestCase(" \"abc\" != \"abc\" ", "false")]
    [TestCase(" \"abc\" != \"abcd\" ", "true")]
    public async Task Can_Ne(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" 1 > 1 ", "false")]
    [TestCase(" 1 > 2 ", "false")]
    [TestCase(" 2 > 1 ", "true")]
    [TestCase(" \"acc\" > \"abc\" ", "true")]
    [TestCase(" true > false ", "true")]
    public async Task Can_Gt(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" 1 >= 1 ", "true")]
    [TestCase(" 1 >= 2 ", "false")]
    [TestCase(" 2 >= 1 ", "true")]
    [TestCase(" \"acc\" >= \"abc\" ", "true")]
    [TestCase(" \"abc\" >= \"abc\" ", "true")]
    [TestCase(" true >= false ", "true")]
    [TestCase(" false >= true ", "false")]
    [TestCase(" false >= false ", "true")]
    public async Task Can_Gte(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" 1 < 1 ", "false")]
    [TestCase(" 1 < 2 ", "true")]
    [TestCase(" 2 < 1 ", "false")]
    [TestCase(" \"acc\" < \"abc\" ", "false")]
    [TestCase(" true < false ", "false")]
    [TestCase(" false < true ", "true")]
    public async Task Can_Lt(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" 1 <= 1 ", "true")]
    [TestCase(" 1 <= 2 ", "true")]
    [TestCase(" 2 <= 1 ", "false")]
    [TestCase(" \"acc\" <= \"abc\" ", "false")]
    [TestCase(" \"abc\" <= \"abc\" ", "true")]
    [TestCase(" true <= false ", "false")]
    [TestCase(" false <= true ", "true")]
    [TestCase(" false <= false ", "true")]
    public async Task Can_Lte(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" 1 and 1 ", "true")]
    [TestCase(" 1 and false ", "false")]
    [TestCase(""" .0.name == "John" and .0.age == 31 """, "true")]
    [TestCase(""" .0.name == "John" and .0.age == 31 and .1.age == 41 """, "true")]
    [TestCase(""" .0.name == "John" and .0.age == 131 and .1.age == 41 """, "false")]
    public async Task Can_And(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" 1 or 1 ", "true")]
    [TestCase(" 1 or false ", "true")]
    [TestCase(" [] or false ", "false")]
    [TestCase(""" .0.name == "Johnny" or .0.age == 31 """, "true")]
    [TestCase(""" .0.name == "Johnny" or .0.age == 131 or .1.age == 41 """, "true")]
    public async Task Can_Or(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" not(1) ", "false")]
    [TestCase(" not(0) ", "true")]
    [TestCase(" not([]) ", "true")]
    [TestCase(" not(.0.age == 31) ", "false")]
    public async Task Can_Not(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" .0 | exists(.age) ", "true")]
    [TestCase(" .0 | exists(.nage) ", "false")]
    [TestCase(" .0 | exists(.address.city) ", "true")]
    [TestCase(" .0 | exists(.address.street) ", "false")]
    public async Task Can_Exists(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" .0.age in [31,41] ", "true")]
    [TestCase(" .0.age in [1,41] ", "false")]
    public async Task Can_In(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" .0.age not in [31,41] ", "false")]
    [TestCase(" .0.age not in [1,41] ", "true")]
    public async Task Can_NotIn(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" if(.0.age == 31, {}, 2) ", "{}")]
    [TestCase(" if(.0.age != 31, {}, 2) ", "2")]
    public async Task Can_If(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" .0.age ", "31")]
    [TestCase(" get(\"0\", \"age\") ", "31")]
    [TestCase(" get() | .1.age ", "41")]
    public async Task Can_Get(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" {} ", "{}")]
    [TestCase(" object({}) ", "{}")]
    [TestCase(""" {"a": 11 } """, """{"a":11}""")]
    [TestCase(""" {"a": .0.age } """, """{"a":31}""")]
    [TestCase(""" object({"a": 11 }) """, """{"a":11}""")]
    [TestCase(""" object({"a": .0.age }) """, """{"a":31}""")]
    public async Task Can_Object(string jsq, string expected)
    {
        await this.Run(jsq, expected);
    }
    
    [TestCase(" [] ", "[]")]
    [TestCase(" [33] ", "[33]")]
    [TestCase(" [.0.age] ", "[31]")]
    [TestCase(" array() ", "[]")]
    [TestCase(" array(33, 12) ", "[33,12]")]
    [TestCase(" array(.0.age) ", "[31]")]
    public async Task Can_Array(string jsq, string expected)
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