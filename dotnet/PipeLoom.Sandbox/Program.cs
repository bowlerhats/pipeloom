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

        using var plan = new WeavePlan(engine);
        plan.RootNode.AppendOperator("log").AppendValue("TEST");
        plan.RootNode.AppendOperator("log").AppendValue("TEST2");
        //plan.RootNode.AppendOperator("const").AppendValue("RES");
        plan.RootNode.AppendValue((decimal)1.33);
        
        var res = await engine.Execute<double>(plan);
        
        Console.WriteLine(res);
        
        Console.WriteLine("Hello, World!");


    }
}