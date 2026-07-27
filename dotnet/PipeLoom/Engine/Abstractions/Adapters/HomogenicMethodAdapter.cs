using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PipeLoom.Engine.Abstractions.Adapters;

internal sealed class HomogenicMethodAdapter<TVariadic, TResult> : MethodAdapter
{
    public override PlOperatorArity Arity => PlOperatorArity.Variadic;
    
    private Converter<Variant, TVariadic> ParamConverter { get; }
    
    private Converter<TResult, Variant> ResultConverter { get; }
    
    public HomogenicMethodAdapter(IPipeLoomEngine engine)
        : base(engine)
    {
        this.ParamConverter = engine.Conversions.FromVariant<TVariadic>();
        this.ResultConverter = engine.Conversions.ToVariant<TResult>();
    }
    
    [OverloadResolutionPriority(1)]
    public HomogenicMethodAdapter(
        IPipeLoomEngine engine,
        Func<ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op
    ) : this(engine)
    {
        this.Seal(this.AsyncCaller(op));
    }
    
    [OverloadResolutionPriority(1)]
    public HomogenicMethodAdapter(
        IPipeLoomEngine engine,
        Func<WeaveStep, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op
    ) : this(engine)
    {
        this.Seal(this.AsyncCallerWithStep(op));
    }
    
    public HomogenicMethodAdapter(
        IPipeLoomEngine engine,
        Func<ReadOnlyMemory<TVariadic>, TResult> op
    ) : this(engine)
    {
        this.Seal(this.SyncCaller(op));
    }
    
    public HomogenicMethodAdapter(
        IPipeLoomEngine engine,
        Func<WeaveStep, ReadOnlyMemory<TVariadic>, TResult> op
    ) : this(engine)
    {
        this.Seal(this.SyncCallerWithStep(op));
    }
    
    private MethodCaller SyncCaller(Func<ReadOnlyMemory<TVariadic>, TResult> op)
    {
        return (state, arguments) =>
        {
            var args = arguments.Span;
            using var recaster = new VariantRecaster<TVariadic>(state.PoolSet.GetArrayPool<TVariadic>(), this.ParamConverter, in args);

            var res = op(recaster.Memory);

            return ValueTask.FromResult(this.ResultConverter(res));
        };
    }
    
    private MethodCaller SyncCallerWithStep(Func<WeaveStep, ReadOnlyMemory<TVariadic>, TResult> op)
    {
        return (state, arguments) =>
        {
            var args = arguments.Span;
            using var recaster = new VariantRecaster<TVariadic>(state.PoolSet.GetArrayPool<TVariadic>(), this.ParamConverter, in args);

            var step = new WeaveStep(state);
            
            var res = op(step, recaster.Memory);

            return ValueTask.FromResult(this.ResultConverter(res));
        };
    }
    
    private MethodCaller AsyncCaller(Func<ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op)
    {
        return async (state, arguments) =>
        {
            var args = arguments.Span;
            using var recaster = new VariantRecaster<TVariadic>(state.PoolSet.GetArrayPool<TVariadic>(), this.ParamConverter, in args);

            var res = await op(recaster.Memory);

            return this.ResultConverter(res);
        };
    }
    
    private MethodCaller AsyncCallerWithStep(Func<WeaveStep, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op)
    {
        return async (state, arguments) =>
        {
            var args = arguments.Span;
            using var recaster = new VariantRecaster<TVariadic>(state.PoolSet.GetArrayPool<TVariadic>(), this.ParamConverter, in args);

            var step = new WeaveStep(state);
            
            var res = await op(step, recaster.Memory);

            return this.ResultConverter(res);
        };
    }
}