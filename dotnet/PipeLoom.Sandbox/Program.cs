using System;
using System.Threading.Tasks;
using PipeLoom.Builder;
using PipeLoom.Engine;

namespace PipeLoom.Sandbox;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var engine = PipeLoomBuilder.Create().Build();

        var plan = new WeavePlan(engine);
        plan.RootNode
            .AppendOperator("log")
            .AppendValue("TEST");

        var res = await engine.Execute(plan);
        
        Console.WriteLine("Hello, World!");
    }
}