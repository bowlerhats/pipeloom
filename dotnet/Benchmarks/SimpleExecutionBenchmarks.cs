using BenchmarkDotNet.Attributes;
using PipeLoom.Builder;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Benchmarks;

[MemoryDiagnoser]
// [DotMemoryDiagnoser]
public class SimpleExecutionBenchmarks
{
    private PipeLoomEngine _engine;
    private WeavePlan _plan;
    private WeavePlan _plan2;
    private int[] _testNumbers;
    
    [GlobalSetup]
    public async ValueTask GlobalSetup()
    {
        _testNumbers = new int[1];
        for (var i = 0; i < _testNumbers.Length; i++)
        {
            _testNumbers[i] = Random.Shared.Next(5);
        }
        
        _engine = PipeLoomBuilder.Create()
            .AddCoreMath()
            .Build();
        
        _plan = new WeavePlan(_engine);
        _plan.RootNode.AppendOperator("sum").AppendValue(Many.Wrap(_testNumbers));
        await _plan.Fuse<long>();
        
        _plan2 = new WeavePlan(_engine);
        var p2Pipe = _plan2.RootNode.AppendOperator("pipe");
        p2Pipe.AppendValue(Many.Wrap(_testNumbers));
        p2Pipe.AppendOperator("sum");
        await _plan2.Fuse<long>();
        
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _plan.Dispose();
        _engine.Dispose();

        _testNumbers = [];
    }

    // [Benchmark]
    public long Sum_Linq()
    {
        return _testNumbers.Sum();
    }

    [Benchmark]
    public async ValueTask<decimal> Sum_Plan()
    {
        return await _engine.Execute<decimal>(_plan);
    }
    
    // [Benchmark]
    public async ValueTask<decimal> Sum_Plan2()
    {
        return await _engine.Execute<decimal>(_plan2);
    }
}