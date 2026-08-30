using BenchmarkDotNet.Attributes;
using PipeLoom.Builder;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Bundles;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Benchmarks;

[MemoryDiagnoser]
public class ManyBenchmarks
{
    private PipeLoomEngine _engine;
    private int[] _testNumbers;

    private WeaveContext _context;

    private Many<int> _manyList;
    private Many<int> _manyLeased;
    
    [GlobalSetup]
    public async ValueTask GlobalSetup()
    {
        _testNumbers = new int[10_000];
        for (var i = 0; i < _testNumbers.Length; i++)
        {
            _testNumbers[i] = Random.Shared.Next(5);
        }
        
        _engine = PipeLoomBuilder.Create().Build();
        
        _context = new WeaveContext(_engine);

        _manyList = Many.Wrap(_testNumbers);
        if (_manyList.AsEnumerable() is not int[])
            throw new PipeLoomException("Expected leased list backing");

        _manyLeased = Many.Create(_testNumbers, _context);
        if (_manyLeased.AsEnumerable() is not LeasedList<int>)
            throw new PipeLoomException("Expected leased list backing");
    }
    
    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _context.Dispose();
        
        // _plan.Dispose();
        _engine.Dispose();

        _testNumbers = [];
    }

    [Benchmark]
    public long Sum_Linq()
    {
        return _testNumbers.Sum();
    }
    
    [Benchmark]
    public long Sum_Linq_NonVector()
    {
        var res = 0L;
        foreach (var num in _testNumbers)
        {
            res += num;
        }

        return res;
    }
    
    [Benchmark]
    public long Sum_Many_List()
    {
        return _manyList.AsEnumerable().Sum();
    }
    
    [Benchmark]
    public long Sum_Many_LeasedList()
    {
        return _manyLeased.AsEnumerable().Sum();
    }
}