using System;
using System.Buffers;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Engine;

public interface IPipeLoomEngine
{
    OperatorRegistry OperatorRegistry { get; }

    void ValidateType<T>();
    ArrayPool<T> GetArrayPool<T>();

    PlTypeDef TypeOf<T>();
}


public class PipeLoomEngine : IPipeLoomEngine
{
    public OperatorRegistry OperatorRegistry { get; }
    
    internal PipeLoomEngine(IEngineConfig config)
    {
        this.OperatorRegistry = new OperatorRegistry(this);
    }

    public virtual void ValidateType<T>()
    {
        throw new NotImplementedException();
    }

    public virtual ArrayPool<T> GetArrayPool<T>()
    {
        return ArrayPool<T>.Shared;
    }

    public PlTypeDef TypeOf<T>()
    {
        throw new NotImplementedException();
    }

    public virtual ValueTask<IBundle<TOutput>> Execute<TInput, TOutput>(IBundle<TInput> input)
    {
        
        throw new NotImplementedException();
    }

    public ValueTask<IBundle<TOutput>> Execute<TOutput>(IBundle<Variant> input)
    {
        return this.Execute<Variant, TOutput>(input);
    }

    public ValueTask<IBundle<Variant>> Execute(IBundle<Variant> input)
    {
        return this.Execute<Variant, Variant>(input);
    }
}