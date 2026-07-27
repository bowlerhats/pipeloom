using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PipeLoom.Engine.Abstractions.Adapters;

public partial class MethodAdapter
{
    protected readonly struct Ignore;
    
    #region Nullary
    
    [OverloadResolutionPriority(1)]
    public static MethodAdapter Nullary<TResult>(IPipeLoomEngine engine, Func<ValueTask<TResult>> op)
    {
        return new GenericMethodAdapter<Ignore, Ignore, Ignore, TResult>(engine, (_, _, _) => op());
    }
    
    public static MethodAdapter Nullary<TResult>(IPipeLoomEngine engine, Func<TResult> op)
    {
        return new GenericMethodAdapter<Ignore, Ignore, Ignore, TResult>(engine, (_, _, _) => op());
    }
    
    [OverloadResolutionPriority(1)]
    public static MethodAdapter Nullary<TResult>(IPipeLoomEngine engine, Func<WeaveStep, ValueTask<TResult>> op)
    {
        return new GenericMethodAdapter<Ignore, Ignore, Ignore, TResult>(engine, (step, _, _, _) => op(step));
    }
    
    public static MethodAdapter Nullary<TResult>(IPipeLoomEngine engine, Func<WeaveStep, TResult> op)
    {
        return new GenericMethodAdapter<Ignore, Ignore, Ignore, TResult>(engine, (step, _, _, _) => op(step));
    }
    
    #endregion
    
    #region Unary
    
    [OverloadResolutionPriority(1)]
    public static MethodAdapter Unary<T1, TResult>(IPipeLoomEngine engine, Func<T1, ValueTask<TResult>> op)
    {
        return new GenericMethodAdapter<T1, Ignore, Ignore, TResult>(engine, (t1, _, _) => op(t1));
    }
    
    public static MethodAdapter Unary<T1, TResult>(IPipeLoomEngine engine, Func<T1, TResult> op)
    {
        return new GenericMethodAdapter<T1, Ignore, Ignore, TResult>(engine, (t1, _, _) => op(t1));
    }
    
    [OverloadResolutionPriority(1)]
    public static MethodAdapter Unary<T1, TResult>(IPipeLoomEngine engine, Func<WeaveStep, T1, ValueTask<TResult>> op)
    {
        return new GenericMethodAdapter<T1, Ignore, Ignore, TResult>(engine, (step, t1, _, _) => op(step, t1));
    }
    
    public static MethodAdapter Unary<T1, TResult>(IPipeLoomEngine engine, Func<WeaveStep, T1, TResult> op)
    {
        return new GenericMethodAdapter<T1, Ignore, Ignore, TResult>(engine, (step, t1, _, _) => op(step, t1));
    }
    
    #endregion
    
    #region Binary
    
    [OverloadResolutionPriority(1)]
    public static MethodAdapter Binary<T1, T2, TResult>(IPipeLoomEngine engine, Func<T1, T2, ValueTask<TResult>> op)
    {
        return new GenericMethodAdapter<T1, T2, Ignore, TResult>(engine, (t1, t2, _) => op(t1, t2));
    }
    
    public static MethodAdapter Binary<T1, T2, TResult>(IPipeLoomEngine engine, Func<T1, T2, TResult> op)
    {
        return new GenericMethodAdapter<T1, T2, Ignore, TResult>(engine, (t1, t2, _) => op(t1, t2));
    }
    
    [OverloadResolutionPriority(1)]
    public static MethodAdapter Binary<T1, T2, TResult>(IPipeLoomEngine engine, Func<WeaveStep, T1, T2, ValueTask<TResult>> op)
    {
        return new GenericMethodAdapter<T1, T2, Ignore, TResult>(engine, (step, t1, t2, _) => op(step, t1, t2));
    }
    
    public static MethodAdapter Binary<T1, T2, TResult>(IPipeLoomEngine engine, Func<WeaveStep, T1, T2, TResult> op)
    {
        return new GenericMethodAdapter<T1, T2, Ignore, TResult>(engine, (step, t1, t2, _) => op(step, t1, t2));
    }
    
    #endregion
    
    #region Ternary
    
    [OverloadResolutionPriority(1)]
    public static MethodAdapter Ternary<T1, T2, T3, TResult>(IPipeLoomEngine engine, Func<T1, T2, T3, ValueTask<TResult>> op)
    {
        return new GenericMethodAdapter<T1, T2, T3, TResult>(engine, op);
    }
    
    public static MethodAdapter Ternary<T1, T2, T3, TResult>(IPipeLoomEngine engine, Func<T1, T2, T3, TResult> op)
    {
        return new GenericMethodAdapter<T1, T2, T3, TResult>(engine, op);
    }
    
    [OverloadResolutionPriority(1)]
    public static MethodAdapter Ternary<T1, T2, T3, TResult>(IPipeLoomEngine engine, Func<WeaveStep, T1, T2, T3, ValueTask<TResult>> op)
    {
        return new GenericMethodAdapter<T1, T2, T3, TResult>(engine, op);
    }
    
    public static MethodAdapter Ternary<T1, T2, T3, TResult>(IPipeLoomEngine engine, Func<WeaveStep, T1, T2, T3, TResult> op)
    {
        return new GenericMethodAdapter<T1, T2, T3, TResult>(engine, op);
    }
    
    #endregion
    
    #region Variadic
    
    [OverloadResolutionPriority(1)]
    public static MethodAdapter Variadic<TVariadic, TResult>(IPipeLoomEngine engine, Func<ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op)
    {
        return new HomogenicMethodAdapter<TVariadic, TResult>(engine, op);
    }
    
    public static MethodAdapter Variadic<TVariadic, TResult>(IPipeLoomEngine engine, Func<ReadOnlyMemory<TVariadic>, TResult> op)
    {
        return new HomogenicMethodAdapter<TVariadic, TResult>(engine, op);
    }
    
    [OverloadResolutionPriority(1)]
    public static MethodAdapter Variadic<TVariadic, TResult>(IPipeLoomEngine engine, Func<WeaveStep, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op)
    {
        return new HomogenicMethodAdapter<TVariadic, TResult>(engine, op);
    }
    
    public static MethodAdapter Variadic<TVariadic, TResult>(IPipeLoomEngine engine, Func<WeaveStep, ReadOnlyMemory<TVariadic>, TResult> op)
    {
        return new HomogenicMethodAdapter<TVariadic, TResult>(engine, op);
    }
    
    [OverloadResolutionPriority(1)]
    public static MethodAdapter Variadic<TImplicit, TVariadic, TResult>(IPipeLoomEngine engine, Func<TImplicit, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op)
    {
        return new ImplicitVariadicMethodAdapter<TImplicit, TVariadic, TResult>(engine, op);
    }
    
    public static MethodAdapter Variadic<TImplicit, TVariadic, TResult>(IPipeLoomEngine engine, Func<TImplicit, ReadOnlyMemory<TVariadic>, TResult> op)
    {
        return new ImplicitVariadicMethodAdapter<TImplicit, TVariadic, TResult>(engine, op);
    }
    
    [OverloadResolutionPriority(1)]
    public static MethodAdapter Variadic<TImplicit, TVariadic, TResult>(IPipeLoomEngine engine, Func<WeaveStep, TImplicit, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op)
    {
        return new ImplicitVariadicMethodAdapter<TImplicit, TVariadic, TResult>(engine, op);
    }
    
    public static MethodAdapter Variadic<TImplicit, TVariadic, TResult>(IPipeLoomEngine engine, Func<WeaveStep, TImplicit, ReadOnlyMemory<TVariadic>, TResult> op)
    {
        return new ImplicitVariadicMethodAdapter<TImplicit, TVariadic, TResult>(engine, op);
    }
    
    #endregion
}