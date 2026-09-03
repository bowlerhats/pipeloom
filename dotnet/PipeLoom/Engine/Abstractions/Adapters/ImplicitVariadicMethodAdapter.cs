using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PipeLoom.Engine.TypeConversions;

namespace PipeLoom.Engine.Abstractions.Adapters;

internal sealed class ImplicitVariadicMethodAdapter<TImplicit, TVariadic, TResult> : MethodAdapter
{
    public override PlOperatorArity Arity => PlOperatorArity.Variadic;
    
    private VariantUnpacker<TImplicit>? ImplicitUnpacker { get; }
    
    private VariantUnpacker<TVariadic> ParamUnpacker { get; }
    
    private PlTypeDef ResultType { get; }
    private VariantPacker<TResult>? ResultPacker { get; }
    
    public ImplicitVariadicMethodAdapter(IPipeLoomEngine engine)
        : base(engine)
    {
        this.ImplicitUnpacker = engine.Conversions.FindCustomVariantUnpacker<TImplicit>();
        
        this.ParamUnpacker
            = engine.Conversions.FindCustomVariantUnpacker<TVariadic>()
              ?? (static (scoped in v) => v.Unpack<TVariadic>());
        
        this.ResultType = engine.TypeOf<TResult>();
        this.ResultPacker = engine.Conversions.FindCustomVariantPacker<TResult>();
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
            
            using var recaster = new VariantRecaster<TVariadic>(state.PoolSet.GetArrayPool<TVariadic>(), this.ParamUnpacker, args[1..]);

            var p1 = this.ImplicitUnpacker is not null ? this.ImplicitUnpacker(in args[0]) : args[0].Unpack<TImplicit>();
            var res = op(p1, recaster.Memory);

            return ValueTask.FromResult(PackResult(in res, this.ResultType, this.ResultPacker));
        };
    }
    
    private MethodCaller SyncCallerWithStep(Func<WeaveStep, TImplicit, ReadOnlyMemory<TVariadic>, TResult> op)
    {
        return (state, arguments) =>
        {
            var args = arguments.Span;
            using var recaster = new VariantRecaster<TVariadic>(state.PoolSet.GetArrayPool<TVariadic>(), this.ParamUnpacker, args[1..]);

            var step = new WeaveStep(state);
            
            var p1 = this.ImplicitUnpacker is not null ? this.ImplicitUnpacker(in args[0]) : args[0].Unpack<TImplicit>();
            var res = op(step, p1, recaster.Memory);

            return ValueTask.FromResult(PackResult(in res, this.ResultType, this.ResultPacker));
        };
    }
    
    private MethodCaller AsyncCaller(Func<TImplicit, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op)
    {
        return async (state, arguments) =>
        {
            var args = arguments.Span;
            using var recaster = new VariantRecaster<TVariadic>(state.PoolSet.GetArrayPool<TVariadic>(), this.ParamUnpacker, args[1..]);

            var p1 = this.ImplicitUnpacker is not null ? this.ImplicitUnpacker(in args[0]) : args[0].Unpack<TImplicit>();
            var res = await op(p1, recaster.Memory);

            return PackResult(in res, this.ResultType, this.ResultPacker);
        };
    }
    
    private MethodCaller AsyncCallerWithStep(Func<WeaveStep, TImplicit, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op)
    {
        return async (state, arguments) =>
        {
            var args = arguments.Span;
            using var recaster = new VariantRecaster<TVariadic>(state.PoolSet.GetArrayPool<TVariadic>(), this.ParamUnpacker, args[1..]);

            var step = new WeaveStep(state);
            
            var p1 = this.ImplicitUnpacker is not null ? this.ImplicitUnpacker(in args[0]) : args[0].Unpack<TImplicit>();
            var res = await op(step, p1, recaster.Memory);

            return PackResult(in res, this.ResultType, this.ResultPacker);
        };
    }
}