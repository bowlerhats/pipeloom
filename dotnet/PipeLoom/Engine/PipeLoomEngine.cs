using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Pools;

namespace PipeLoom.Engine;

// [Flags]
// public enum PipeLoomConversionFlags
// {
//     None = 0,
//     
// }

public class PipeLoomEngine : IPipeLoomEngine
{
    public IPlConversionMap Conversions => throw new NotImplementedException();
    
    public IPlWellknown WellKnown { get; }
    
    public OperatorRegistry OperatorRegistry { get; }
    
    internal IObjectPool<MemCachedPoolSet> PoolSets { get; }

    private TypeMap _typeMap;
    
    internal PipeLoomEngine(IEngineConfig config)
    {
        this.OperatorRegistry = new OperatorRegistry(this);
        this.WellKnown = new PlWellknown{ Engine = this };
        _typeMap = new TypeMap(this);

        this.PoolSets = new ObjectPool<MemCachedPoolSet>(
            pool => new MemCachedPoolSet(pool),
            MagicNumbers.EnginePoolSetSize);
    }

    public virtual void ValidateType<T>()
    {
        throw new NotImplementedException();
    }

    public PlTypeDef TypeOf<T>()
    {
        return _typeMap.TypeOf<T>();
    }

    public bool IsConvertible(PlTypeDef from, PlTypeDef to)
    {
        if (from == to)
            return true;
        
        throw new NotImplementedException();
    }

    public Variant ConvertValue(Variant value, PlTypeDef target)
    {
        if (value.Tag == target)
            return value;
        
        throw new NotImplementedException();
    }

    public T GetType<T>()
        where T : PlTypeDef
    {
        throw new NotImplementedException();
    }

    public PlTypeDef CommonBaseOf(IEnumerable<PlTypeDef> types)
    {
        throw new NotImplementedException();
    }

    public PlOperatorArity? GuessArity(IEnumerable<PlTypeDef> args)
    {
        throw new NotImplementedException();
    }

    public PlOperatorClass GetOperatorClass(string operatorName)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<TOutput> Execute<TOutput>(WeavePlan plan)
    {
        var outputType = this.TypeOf<TOutput>();
        
        if (!plan.IsFused)
        {
            await plan.Fuse<TOutput>();
        }
        
        if (!this.IsConvertible(plan.OutputType, outputType))
        {
            throw new PipeLoomException($"Plan is fused to type '{plan.OutputType.Name}' which is not convertible to requested type '{outputType.Name}'");
        }

        using var context = new WeaveContext(this, plan);

        var result = await context.Step();

        result = this.ConvertValue(result, outputType);

        return result.Unpack<TOutput>(reinterpret: true);
    }

    public ValueTask<IBundle<Variant>> Execute(WeavePlan plan)
    {
        return this.Execute<IBundle<Variant>>(plan);
    }
}