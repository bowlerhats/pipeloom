using System;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ConsoleAppFramework;
using PipeLoom.Builder;
using PipeLoom.Engine;
using PipeLoom.JsonQuery;

ConsoleApp.Run(args, Commands.Eval);

internal static class Commands
{
    /// <summary>
    /// Run a json query against input
    /// </summary>
    /// <param name="input">-f, File to read data from</param>
    /// <param name="output">-o, File to write to</param>
    /// <param name="parseOnly">-p, Only parse the query and write the result to stdout</param>
    /// <param name="testData"></param>
    /// <param name="testQuery"></param>
    /// <param name="stackTrace">Shows exception stack traces</param>
    /// <param name="queryParts">The json query</param>
    public static async Task<int> Eval(
        string? input = null,
        string? output = null,
        bool parseOnly = false,
        bool testData = false,
        bool testQuery = false,
        bool stackTrace = false,
        [Argument] params string[] queryParts
    )
    {
        try
        {
            var query = testQuery
                ? "{ \"a\": [.0.person]}"
                : string.Join(' ', queryParts);

            if (string.IsNullOrWhiteSpace(query))
            {
                await Console.Error.WriteLineAsync("Empty query");
                return 1;
            }

            var parsed = JsonQueryUtils.Parse(query);
            
            if (parsed is null)
            {
                await Console.Error.WriteLineAsync("Failed to parse");
                return -1;
            }
            
            if (parseOnly)
            {
                Console.WriteLine(parsed.ToJsonString());
                return 0;
            }

            using var engine = PipeLoomBuilder.Create()
                .AddJsonQuery()
                .Build();

            var plan = new WeavePlan(engine);
            plan.WithInputCarry<JsonNode?>()
                .AppendJsonQuery(parsed);
            
            await plan.Fuse<JsonNode>();

            string json;
            if (testData)
            {
                json = """
                       [
                        { "person": { "name": "John", "age": 31 } }
                       ]
                       """;
            }
            else if (string.IsNullOrWhiteSpace(input))
            {
                using var reader = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding);
                json = await reader.ReadToEndAsync();
            }
            else
            {
                json = await File.ReadAllTextAsync(input);
            }

            var inputData = JsonNode.Parse(json);

            var result = await engine.Execute<JsonNode?, JsonNode?>(plan, inputData);

            Console.WriteLine(result?.ToJsonString() ?? "null");
        }
        catch (Exception ex)
        {
            if (!stackTrace)
            {
                await Console.Error.WriteLineAsync($"ERROR({ex.GetType().Name}) {ex.Message}");
            }
            else
            {
                throw;
            }
        }
        
        return 0;
    }
}