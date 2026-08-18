using BenchmarkDotNet.Attributes;
using PipeLoom.Builder;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;

namespace Benchmarks;

[MemoryDiagnoser]
public class SimpleExecutionBenchmarks
{
    private PipeLoomEngine _engine;
    private WeavePlan _plan;
    private int[] _testNumbers;
    
    [GlobalSetup]
    public async ValueTask GlobalSetup()
    {
        _testNumbers = new int[10_000];
        for (var i = 0; i < _testNumbers.Length; i++)
        {
            _testNumbers[i] = Random.Shared.Next(5);
        }
        
        _engine = PipeLoomBuilder.Create().Build();
        _plan = new WeavePlan(_engine);
        _plan.RootNode.AppendOperator("sum").AppendValue(new Many<int>(_testNumbers));
        await _plan.Fuse<int>();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _plan.Dispose();
        _engine.Dispose();

        _testNumbers = [];
    }

    [Benchmark]
    public int Sum_Linq()
    {
        return _testNumbers.Sum();
    }

    [Benchmark]
    public async ValueTask<int> Sum_Plan()
    {
        return await _engine.Execute<int>(_plan);
    }
}