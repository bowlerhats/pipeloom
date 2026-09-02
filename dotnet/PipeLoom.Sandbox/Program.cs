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
            .AddCoreControlFlow()
            .Build();

        using var plan = new WeavePlan(engine);
        plan.RootNode.AppendOperator("log").AppendValue("TEST");
        plan.RootNode.AppendOperator("log").AppendValue("TEST2");
        
        var ifcond = plan.RootNode.AppendOperator("if");
        ifcond.AppendOperator("isNull").AppendValue(1);
        ifcond.AppendOperator("log").AppendValue("IF-THEN");
        ifcond.AppendOperator("log").AppendValue("IF-ELSE");
        
        plan.RootNode.AppendOperator("sum").AppendValue(Many.Wrap<ulong>([1, 2, 3]));
        
        var res = await engine.Execute<ulong>(plan);
        
        Console.WriteLine(res);
        
        Console.WriteLine("Hello, World!");
    }
}