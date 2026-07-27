using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PipeLoom.Engine.Abstractions.Adapters;

internal sealed class ImplicitVariadicMethodAdapter<TImplicit, TVariadic, TResult> : MethodAdapter
{
    public override PlOperatorArity Arity => PlOperatorArity.Variadic;
    
    private Converter<Variant, TImplicit> ImplicitConverter { get; }
    
    private Converter<Variant, TVariadic> ParamConverter { get; }
    
    private Converter<TResult, Variant> ResultConverter { get; }
    
    public ImplicitVariadicMethodAdapter(IPipeLoomEngine engine)
        : base(engine)
    {
        this.ImplicitConverter = engine.Conversions.FromVariant<TImplicit>();
        this.ParamConverter = engine.Conversions.FromVariant<TVariadic>();
        this.ResultConverter = engine.Conversions.ToVariant<TResult>();
    }
    
    [OverloadResolutionPriority(1)]
    public ImplicitVariadicMethodAdapter(
        IPipeLoomEngine engine,
        Func<TImplicit, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op
    ) : this(engine)
    {
        this.Seal(this.AsyncCaller(op));
    }
    
    [OverloadResolutionPriority(1)]
    public ImplicitVariadicMethodAdapter(
        IPipeLoomEngine engine,
        Func<WeaveStep, TImplicit, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op
    ) : this(engine)
    {
        this.Seal(this.AsyncCallerWithStep(op));
    }
    
    public ImplicitVariadicMethodAdapter(
        IPipeLoomEngine engine,
        Func<TImplicit, ReadOnlyMemory<TVariadic>, TResult> op
    ) : this(engine)
    {
        this.Seal(this.SyncCaller(op));
    }
    
    public ImplicitVariadicMethodAdapter(
        IPipeLoomEngine engine,
        Func<WeaveStep, TImplicit, ReadOnlyMemory<TVariadic>, TResult> op
    ) : this(engine)
    {
        this.Seal(this.SyncCallerWithStep(op));
    }
    
    private MethodCaller SyncCaller(Func<TImplicit, ReadOnlyMemory<TVariadic>, TResult> op)
    {
        return (state, arguments) =>
        {
            var args = arguments.Span;
            
            using var recaster = new VariantRecaster<TVariadic>(state.PoolSet.GetArrayPool<TVariadic>(), this.ParamConverter, args[1..]);

            var p1 = this.ImplicitConverter(args[0]);
            var res = op(p1, recaster.Memory);

            return ValueTask.FromResult(this.ResultConverter(res));
        };
    }
    
    private MethodCaller SyncCallerWithStep(Func<WeaveStep, TImplicit, ReadOnlyMemory<TVariadic>, TResult> op)
    {
        return (state, arguments) =>
        {
            var args = arguments.Span;
            using var recaster = new VariantRecaster<TVariadic>(state.PoolSet.GetArrayPool<TVariadic>(), this.ParamConverter, args[1..]);

            var step = new WeaveStep(state);
            
            var p1 = this.ImplicitConverter(args[0]);
            var res = op(step, p1, recaster.Memory);

            return ValueTask.FromResult(this.ResultConverter(res));
        };
    }
    
    private MethodCaller AsyncCaller(Func<TImplicit, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op)
    {
        return async (state, arguments) =>
        {
            var args = arguments.Span;
            using var recaster = new VariantRecaster<TVariadic>(state.PoolSet.GetArrayPool<TVariadic>(), this.ParamConverter, args[1..]);

            var p1 = this.ImplicitConverter(args[0]);
            var res = await op(p1, recaster.Memory);

            return this.ResultConverter(res);
        };
    }
    
    private MethodCaller AsyncCallerWithStep(Func<WeaveStep, TImplicit, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op)
    {
        return async (state, arguments) =>
        {
            var args = arguments.Span;
            using var recaster = new VariantRecaster<TVariadic>(state.PoolSet.GetArrayPool<TVariadic>(), this.ParamConverter, in args);

            var step = new WeaveStep(state);
            
            var p1 = this.ImplicitConverter(args[0]);
            var res = await op(step, p1, recaster.Memory);

            return this.ResultConverter(res);
        };
    }
}