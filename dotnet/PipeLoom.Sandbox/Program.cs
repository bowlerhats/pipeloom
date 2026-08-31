using System;
using System.Threading.Tasks;
using PipeLoom.Builder;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Sandbox;

internal class Program
{
    private static async Task Main()
    {
        var engine = PipeLoomBuilder.Create()
            .AddExtendedMath()
            .Build();

        using var plan = new WeavePlan(engine);
        plan.RootNode.AppendOperator("log").AppendValue("TEST");
        plan.RootNode.AppendOperator("log").AppendValue("TEST2");
        
        //plan.RootNode.AppendOperator("const").AppendValue("RES");
        // plan.RootNode.AppendOperator("sum").AppendValue(new Many<int>([1, 2, 3]));
        //plan.RootNode.AppendValue((decimal)1.33);
        
        var pipe = plan.RootNode.AppendOperator("pipe");
        pipe.AppendValue(Many.Wrap<ulong>([1, 2, 3]));
        pipe.AppendOperator("sum");
            //.AppendValue(new Many<short>([1, 2, 3]));
        
        var res = await engine.Execute<ulong>(plan);
        
        Console.WriteLine(res);
        
        Console.WriteLine("Hello, World!");
    }
}