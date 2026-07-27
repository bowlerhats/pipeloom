using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine.Abstractions.Adapters;

internal sealed class GenericMethodAdapter<T1, T2, T3, TResult> : MethodAdapter
{
    private static int CountArity { get; } =
        (typeof(T1) == typeof(Ignore) ? 0 : 1) +
        (typeof(T2) == typeof(Ignore) ? 0 : 1) +
        (typeof(T3) == typeof(Ignore) ? 0 : 1);

    public override PlOperatorArity Arity => PlOperatorArityExtensions.Infer(CountArity);

    private Converter<Variant, T1> P1Converter { get; }
    private Converter<Variant, T2> P2Converter { get; }
    private Converter<Variant, T3> P3Converter { get; }
    
    private Converter<TResult, Variant> ResultConverter { get; }

    private GenericMethodAdapter(IPipeLoomEngine engine)
        : base(engine)
    {
        if (this.Arity == PlOperatorArity.Variadic)
            // This should never happen, but better guard against it if Infer borks...
            throw new PipeLoomException("Attempted to use a generic adapter for a variadic function?!");
        
        this.P1Converter = static _ => throw IgnoredConversion();
        this.P2Converter = static _ => throw IgnoredConversion();
        this.P3Converter = static _ => throw IgnoredConversion();
        this.ResultConverter = engine.Conversions.ToVariant<TResult>();

        if (typeof(T1) != typeof(Ignore))
        {
            this.P1Converter = engine.Conversions.FromVariant<T1>();
        }

        if (typeof(T2) != typeof(Ignore))
        {
            this.P2Converter = engine.Conversions.FromVariant<T2>();
        }
        
        if (typeof(T3) != typeof(Ignore))
        {
            this.P3Converter = engine.Conversions.FromVariant<T3>();
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
            var p1 = typeof(T1) == typeof(Ignore) ? default! : this.P1Converter(args[0]);
            var p2 = typeof(T2) == typeof(Ignore) ? default! : this.P2Converter(args[1]);
            var p3 = typeof(T3) == typeof(Ignore) ? default! : this.P3Converter(args[2]);

            var res = op(p1, p2, p3);

            return ValueTask.FromResult(this.ResultConverter(res));
        };
    }
    
    private MethodCaller SyncCallerWithStep(Func<WeaveStep, T1, T2, T3, TResult> op)
    {
        return (state, arguments) =>
        {
            Debug.Assert(arguments.Length == CountArity, "Mismatched arity between user function and method adapter");
            
            var args = arguments.Span;
            var p1 = typeof(T1) == typeof(Ignore) ? default! : this.P1Converter(args[0]);
            var p2 = typeof(T2) == typeof(Ignore) ? default! : this.P2Converter(args[1]);
            var p3 = typeof(T3) == typeof(Ignore) ? default! : this.P3Converter(args[2]);

            var step = new WeaveStep(state);

            var res = op(step, p1, p2, p3);

            return ValueTask.FromResult(this.ResultConverter(res));
        };
    }
    
    private MethodCaller AsyncCaller(Func<T1, T2, T3, ValueTask<TResult>> op)
    {
        return async (_, arguments) =>
        {
            Debug.Assert(arguments.Length == CountArity, "Mismatched arity between user function and method adapter");
            
            var args = arguments.Span;
            var p1 = typeof(T1) == typeof(Ignore) ? default! : this.P1Converter(args[0]);
            var p2 = typeof(T2) == typeof(Ignore) ? default! : this.P2Converter(args[1]);
            var p3 = typeof(T3) == typeof(Ignore) ? default! : this.P3Converter(args[2]);

            var res = await op(p1, p2, p3);

            return this.ResultConverter(res);
        };
    }
    
    private MethodCaller AsyncCallerWithStep(Func<WeaveStep, T1, T2, T3, ValueTask<TResult>> op)
    {
        return async (state, arguments) =>
        {
            Debug.Assert(arguments.Length == CountArity, "Mismatched arity between user function and method adapter");
            
            var args = arguments.Span;
            var p1 = typeof(T1) == typeof(Ignore) ? default! : this.P1Converter(args[0]);
            var p2 = typeof(T2) == typeof(Ignore) ? default! : this.P2Converter(args[1]);
            var p3 = typeof(T3) == typeof(Ignore) ? default! : this.P3Converter(args[2]);

            var step = new WeaveStep(state);

            var res = await op(step, p1, p2, p3);

            return this.ResultConverter(res);
        };
    }

    private static PipeLoomException IgnoredConversion()
    {
        return new PipeLoomException("Attempted conversion of ignored type");
    }
}