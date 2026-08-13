using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.TypeConversions;

namespace PipeLoom.Engine.Abstractions.Adapters;

internal sealed class GenericMethodAdapter<T1, T2, T3, TResult> : MethodAdapter
{
    private static int CountArity { get; } =
        (typeof(T1) == typeof(Ignore) ? 0 : 1) +
        (typeof(T2) == typeof(Ignore) ? 0 : 1) +
        (typeof(T3) == typeof(Ignore) ? 0 : 1);

    public override PlOperatorArity Arity => PlOperatorArityExtensions.Infer(CountArity);

    private VariantUnpacker<T1>? P1Unpacker { get; }
    private VariantUnpacker<T2>? P2Unpacker { get; }
    private VariantUnpacker<T3>? P3Unpacker { get; }
    
    private PlTypeDef ResultType { get; }
    private VariantPacker<TResult>? ResultPacker { get; }
    
    private GenericMethodAdapter(IPipeLoomEngine engine)
        : base(engine)
    {
        if (this.Arity == PlOperatorArity.Variadic)
            // This should never happen, but better guard against it if Infer borks...
            throw new PipeLoomException("Attempted to use a generic adapter for a variadic function?!");

        this.ResultType = engine.TypeOf<TResult>();
        this.ResultPacker = engine.Conversions.FindCustomVariantPacker<TResult>();

        if (typeof(T1) != typeof(Ignore))
        {
            this.P1Unpacker = engine.Conversions.FindCustomVariantUnpacker<T1>();
        }

        if (typeof(T2) != typeof(Ignore))
        {
            this.P2Unpacker = engine.Conversions.FindCustomVariantUnpacker<T2>();
        }
        
        if (typeof(T3) != typeof(Ignore))
        {
            this.P3Unpacker = engine.Conversions.FindCustomVariantUnpacker<T3>();
        }
    }
    
    [OverloadResolutionPriority(1)]
    public GenericMethodAdapter(
        IPipeLoomEngine engine,
        Func<T1, T2, T3, ValueTask<TResult>> op
    ) : this(engine)
    {
        this.Seal(this.AsyncCaller(op));
    }
    
    [OverloadResolutionPriority(1)]
    public GenericMethodAdapter(
        IPipeLoomEngine engine,
        Func<WeaveStep, T1, T2, T3, ValueTask<TResult>> op
    ) : this(engine)
    {
        this.Seal(this.AsyncCallerWithStep(op));
    }
    
    public GenericMethodAdapter(
        IPipeLoomEngine engine,
        Func<T1, T2, T3, TResult> op
    ) : this(engine)
    {
        this.Seal(this.SyncCaller(op));
    }
    
    public GenericMethodAdapter(
        IPipeLoomEngine engine,
        Func<WeaveStep, T1, T2, T3, TResult> op
    ) : this(engine)
    {
        this.Seal(this.SyncCallerWithStep(op));
    }

    private MethodCaller SyncCaller(Func<T1, T2, T3, TResult> op)
    {
        return (_, arguments) =>
        {
            Debug.Assert(arguments.Length == CountArity, "Mismatched arity between user function and method adapter");
            
            var args = arguments.Span;
            var p1 = typeof(T1) == typeof(Ignore) ? default! : this.P1Unpacker is not null ? this.P1Unpacker(in args[0]) : args[0].Unpack<T1>();
            var p2 = typeof(T2) == typeof(Ignore) ? default! : this.P2Unpacker is not null ? this.P2Unpacker(in args[1]) : args[1].Unpack<T2>();
            var p3 = typeof(T3) == typeof(Ignore) ? default! : this.P3Unpacker is not null ? this.P3Unpacker(in args[2]) : args[2].Unpack<T3>();

            var res = op(p1, p2, p3);

            return ValueTask.FromResult(PackResult(in res, this.ResultType, this.ResultPacker));
        };
    }
    
    private MethodCaller SyncCallerWithStep(Func<WeaveStep, T1, T2, T3, TResult> op)
    {
        return (state, arguments) =>
        {
            Debug.Assert(arguments.Length == CountArity, "Mismatched arity between user function and method adapter");
            
            var args = arguments.Span;
            var p1 = typeof(T1) == typeof(Ignore) ? default! : this.P1Unpacker is not null ? this.P1Unpacker(in args[0]) : args[0].Unpack<T1>();
            var p2 = typeof(T2) == typeof(Ignore) ? default! : this.P2Unpacker is not null ? this.P2Unpacker(in args[1]) : args[1].Unpack<T2>();
            var p3 = typeof(T3) == typeof(Ignore) ? default! : this.P3Unpacker is not null ? this.P3Unpacker(in args[2]) : args[2].Unpack<T3>();

            var step = new WeaveStep(state);

            var res = op(step, p1, p2, p3);

            return ValueTask.FromResult(PackResult(in res, this.ResultType, this.ResultPacker));
        };
    }
    
    private MethodCaller AsyncCaller(Func<T1, T2, T3, ValueTask<TResult>> op)
    {
        return async (_, arguments) =>
        {
            Debug.Assert(arguments.Length == CountArity, "Mismatched arity between user function and method adapter");
            
            var args = arguments.Span;
            var p1 = typeof(T1) == typeof(Ignore) ? default! : this.P1Unpacker is not null ? this.P1Unpacker(in args[0]) : args[0].Unpack<T1>();
            var p2 = typeof(T2) == typeof(Ignore) ? default! : this.P2Unpacker is not null ? this.P2Unpacker(in args[1]) : args[1].Unpack<T2>();
            var p3 = typeof(T3) == typeof(Ignore) ? default! : this.P3Unpacker is not null ? this.P3Unpacker(in args[2]) : args[2].Unpack<T3>();

            var res = await op(p1, p2, p3);

            return PackResult(in res, this.ResultType, this.ResultPacker);
        };
    }
    
    private MethodCaller AsyncCallerWithStep(Func<WeaveStep, T1, T2, T3, ValueTask<TResult>> op)
    {
        return async (state, arguments) =>
        {
            Debug.Assert(arguments.Length == CountArity, "Mismatched arity between user function and method adapter");
            
            var args = arguments.Span;
            var p1 = typeof(T1) == typeof(Ignore) ? default! : this.P1Unpacker is not null ? this.P1Unpacker(in args[0]) : args[0].Unpack<T1>();
            var p2 = typeof(T2) == typeof(Ignore) ? default! : this.P2Unpacker is not null ? this.P2Unpacker(in args[1]) : args[1].Unpack<T2>();
            var p3 = typeof(T3) == typeof(Ignore) ? default! : this.P3Unpacker is not null ? this.P3Unpacker(in args[2]) : args[2].Unpack<T3>();

            var step = new WeaveStep(state);

            var res = await op(step, p1, p2, p3);

            return PackResult(in res, this.ResultType, this.ResultPacker);
        };
    }
}