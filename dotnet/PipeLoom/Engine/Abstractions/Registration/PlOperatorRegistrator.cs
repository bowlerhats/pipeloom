using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Engine.Abstractions.Registration;

public abstract class PlOperatorRegistrator
{
    public IPipeLoomEngine Engine { get; }
    public PlOperatorClass OperatorClass { get; }
    
    protected PlOperatorRegistrator(PlOperatorClass operatorClass)
    {
        this.OperatorClass = operatorClass;
        this.Engine = operatorClass.Engine;
    }

    // public VariadicRegistrator<T> AsVariadic<T>()
    // {
    //     return new VariadicRegistrator<T>(this);
    // }
    
    protected virtual PlOperatorRegistrator Register(OperatorHandler handler)
    {
        this.OperatorClass.AddHandler(handler);
        
        return this;
    }
    
    #region Nullary
    
    public virtual NullaryRegistrator AsNullary(HandlerConfig<NullaryHandler> config = default)
    {
        return new NullaryRegistrator(this, config);
    }
    
    protected virtual PlOperatorRegistrator Nullary(NullaryHandler handler, HandlerConfig<NullaryHandler> config = default)
    {
        config.Apply(handler);

        return this.Register(handler);
    }
    
    public virtual PlOperatorRegistrator Nullary(PlOperatorDelegates.NullaryFunction op, HandlerConfig<NullaryHandler> config = default)
    {
        return this.Nullary(new NullaryHandler(this.OperatorClass, op), config);
    }
    
    public virtual PlOperatorRegistrator Nullary(PlOperatorDelegates.NullaryFunctionWithStep op, HandlerConfig<NullaryHandler> config = default)
    {
        return this.Nullary(new NullaryHandler(this.OperatorClass, op), config);
    }
    
    public virtual PlOperatorRegistrator Nullary(PlOperatorDelegates.NullaryFunctionAsync op, HandlerConfig<NullaryHandler> config = default)
    {
        return this.Nullary(new NullaryHandler(this.OperatorClass, op), config);
    }
    
    public virtual PlOperatorRegistrator Nullary(PlOperatorDelegates.NullaryFunctionAsyncWithStep op, HandlerConfig<NullaryHandler> config = default)
    {
        return this.Nullary(new NullaryHandler(this.OperatorClass, op), config);
    }
    
    #endregion
    
    #region Unary
    
    public virtual UnaryRegistrator<T> AsUnary<T>(HandlerConfig<UnaryHandler> config = default)
    {
        return new UnaryRegistrator<T>(this, config);
    }
    
    protected virtual PlOperatorRegistrator Unary(UnaryHandler handler, HandlerConfig<UnaryHandler> config = default)
    {
        config.Apply(handler);

        return this.Register(handler);
    }
    
    public virtual PlOperatorRegistrator Unary(PlOperatorDelegates.UnaryFunction op, HandlerConfig<UnaryHandler> config = default)
    {
        return this.Unary(new UnaryHandler(this.OperatorClass, op), config);
    }
    
    public virtual PlOperatorRegistrator Unary(PlOperatorDelegates.UnaryFunctionWithStep op, HandlerConfig<UnaryHandler> config = default)
    {
        return this.Unary(new UnaryHandler(this.OperatorClass, op), config);
    }
    
    public virtual PlOperatorRegistrator Unary(PlOperatorDelegates.UnaryFunctionAsync op, HandlerConfig<UnaryHandler> config = default)
    {
        return this.Unary(new UnaryHandler(this.OperatorClass, op), config);
    }
    
    public virtual PlOperatorRegistrator Unary(PlOperatorDelegates.UnaryFunctionAsyncWithStep op, HandlerConfig<UnaryHandler> config = default)
    {
        return this.Unary(new UnaryHandler(this.OperatorClass, op), config);
    }
    
    #endregion
    
    #region Binary
    
    public virtual BinaryRegistrator<T1, T2> AsBinary<T1, T2>(HandlerConfig<BinaryHandler> config = default)
    {
        return new BinaryRegistrator<T1, T2>(this, config);
    }
    
    public virtual BinaryRegistrator<TSymmetric, TSymmetric> AsBinary<TSymmetric>(HandlerConfig<BinaryHandler> config = default)
    {
        return new BinaryRegistrator<TSymmetric, TSymmetric>(this, config);
    }
    
    protected virtual PlOperatorRegistrator Binary(BinaryHandler handler, HandlerConfig<BinaryHandler> config = default)
    {
        config.Apply(handler);

        return this.Register(handler);
    }
    
    public virtual PlOperatorRegistrator Binary(PlOperatorDelegates.BinaryFunction op, HandlerConfig<BinaryHandler> config = default)
    {
        return this.Binary(new BinaryHandler(this.OperatorClass, op), config);
    }
    
    public virtual PlOperatorRegistrator Binary(PlOperatorDelegates.BinaryFunctionWithStep op, HandlerConfig<BinaryHandler> config = default)
    {
        return this.Binary(new BinaryHandler(this.OperatorClass, op), config);
    }
    
    public virtual PlOperatorRegistrator Binary(PlOperatorDelegates.BinaryFunctionAsync op, HandlerConfig<BinaryHandler> config = default)
    {
        return this.Binary(new BinaryHandler(this.OperatorClass, op), config);
    }
    
    public virtual PlOperatorRegistrator Binary(PlOperatorDelegates.BinaryFunctionAsyncWithStep op, HandlerConfig<BinaryHandler> config = default)
    {
        return this.Binary(new BinaryHandler(this.OperatorClass, op), config);
    }
    
    #endregion
    
    #region Ternary
    
    public virtual TernaryRegistrator<T1, T2, T3> AsTernary<T1, T2, T3>(HandlerConfig<TernaryHandler> config = default)
    {
        return new TernaryRegistrator<T1, T2, T3>(this, config);
    }
    
    protected virtual PlOperatorRegistrator Ternary(TernaryHandler handler, HandlerConfig<TernaryHandler> config = default)
    {
        config.Apply(handler);

        return this.Register(handler);
    }
    
    public virtual PlOperatorRegistrator Ternary(PlOperatorDelegates.TernaryFunction op, HandlerConfig<TernaryHandler> config = default)
    {
        return this.Ternary(new TernaryHandler(this.OperatorClass, op), config);
    }
    
    public virtual PlOperatorRegistrator Ternary(PlOperatorDelegates.TernaryFunctionWithStep op, HandlerConfig<TernaryHandler> config = default)
    {
        return this.Ternary(new TernaryHandler(this.OperatorClass, op), config);
    }
    
    public virtual PlOperatorRegistrator Ternary(PlOperatorDelegates.TernaryFunctionAsync op, HandlerConfig<TernaryHandler> config = default)
    {
        return this.Ternary(new TernaryHandler(this.OperatorClass, op), config);
    }
    
    public virtual PlOperatorRegistrator Ternary(PlOperatorDelegates.TernaryFunctionAsyncWithStep op, HandlerConfig<TernaryHandler> config = default)
    {
        return this.Ternary(new TernaryHandler(this.OperatorClass, op), config);
    }
    
    #endregion
    
    #region Variadic
    
    public virtual VariadicRegistrator<T> AsVariadic<T>(HandlerConfig<VariadicHandler> config = default)
    {
        return new VariadicRegistrator<T>(this, config);
    }
    
    protected virtual PlOperatorRegistrator Variadic(VariadicHandler handler, HandlerConfig<VariadicHandler> config = default)
    {
        config.Apply(handler);

        return this.Register(handler);
    }
    
    public virtual PlOperatorRegistrator Variadic(PlOperatorDelegates.VariadicFunction op, HandlerConfig<VariadicHandler> config = default)
    {
        return this.Variadic(new VariadicHandler(this.OperatorClass, op), config);
    }
    
    public virtual PlOperatorRegistrator Variadic(PlOperatorDelegates.VariadicFunctionWithStep op, HandlerConfig<VariadicHandler> config = default)
    {
        return this.Variadic(new VariadicHandler(this.OperatorClass, op), config);
    }
    
    public virtual PlOperatorRegistrator Variadic(PlOperatorDelegates.VariadicFunctionAsync op, HandlerConfig<VariadicHandler> config = default)
    {
        return this.Variadic(new VariadicHandler(this.OperatorClass, op), config);
    }
    
    public virtual PlOperatorRegistrator Variadic(PlOperatorDelegates.VariadicFunctionAsyncWithStep op, HandlerConfig<VariadicHandler> config = default)
    {
        return this.Variadic(new VariadicHandler(this.OperatorClass, op), config);
    }
    
    #endregion
    
    //#endregion
    
    
    // #region Binary
    //
    // [OverloadResolutionPriority(2)]
    // public virtual PlOperatorRegistrator Binary(PlOperatorDelegates.BinaryOp op)
    // {
    //     return this;
    // }
    //
    // [OverloadResolutionPriority(2)]
    // public virtual PlOperatorRegistrator Binary(PlOperatorDelegates.BinaryOpWithSingleStep op)
    // {
    //     return this;
    // }
    //
    // [OverloadResolutionPriority(2)]
    // public virtual PlOperatorRegistrator Binary(PlOperatorDelegates.BinaryOpAsync op)
    // {
    //     return this;
    // }
    //
    // [OverloadResolutionPriority(2)]
    // public virtual PlOperatorRegistrator Binary(PlOperatorDelegates.BinaryOpWithSingleStepAsync op)
    // {
    //     return this;
    // }
    //
    // [OverloadResolutionPriority(1)]
    // public virtual PlOperatorRegistrator Binary<TResult, T1, T2>(Func<T1, T2, ValueTask<TResult>> op)
    // {
    //     this.Engine.ValidateType<TResult>();
    //     this.Engine.ValidateType<T1>();
    //     this.Engine.ValidateType<T2>();
    //     
    //     return this.Binary(async (t1, t2) => Variant.From(await op(t1.Unpack<T1>(), t2.Unpack<T2>())));
    // }
    //
    // [OverloadResolutionPriority(1)]
    // public virtual PlOperatorRegistrator Binary<TResult, T1, T2>(Func<MapperStep, T1, T2, ValueTask<TResult>> op)
    // {
    //     this.Engine.ValidateType<TResult>();
    //     this.Engine.ValidateType<T1>();
    //     this.Engine.ValidateType<T2>();
    //     
    //     return this.Binary(async (step, t1, t2) => Variant.From(await op(step, t1.Unpack<T1>(), t2.Unpack<T2>())));
    // }
    //
    // public virtual PlOperatorRegistrator Binary<TResult, T1, T2>(Func<T1, T2, TResult> op)
    // {
    //     this.Engine.ValidateType<TResult>();
    //     this.Engine.ValidateType<T1>();
    //     this.Engine.ValidateType<T2>();
    //     
    //     return this.Binary((scoped in t1, scoped in t2) => Variant.From(op(t1.Unpack<T1>(), t2.Unpack<T2>())));
    // }
    //
    // public virtual PlOperatorRegistrator Binary<TResult, T1, T2>(Func<MapperStep, T1, T2, TResult> op)
    // {
    //     this.Engine.ValidateType<TResult>();
    //     this.Engine.ValidateType<T1>();
    //     this.Engine.ValidateType<T2>();
    //     
    //     return this.Binary((scoped in step, scoped in t1, scoped in t2) => Variant.From(op(step, t1.Unpack<T1>(), t2.Unpack<T2>())));
    // }
    //
    // #endregion
    //
    // #region Ternary
    //
    // [OverloadResolutionPriority(2)]
    // public virtual PlOperatorRegistrator Ternary(PlOperatorDelegates.TernaryOp op)
    // {
    //     return this;
    // }
    //
    // [OverloadResolutionPriority(2)]
    // public virtual PlOperatorRegistrator Ternary(PlOperatorDelegates.TernaryOpWithSingleStep op)
    // {
    //     return this;
    // }
    //
    // [OverloadResolutionPriority(2)]
    // public virtual PlOperatorRegistrator Ternary(PlOperatorDelegates.TernaryOpAsync op)
    // {
    //     return this;
    // }
    //
    // [OverloadResolutionPriority(2)]
    // public virtual PlOperatorRegistrator Ternary(PlOperatorDelegates.TernaryOpWithSingleStepAsync op)
    // {
    //     return this;
    // }
    //
    // [OverloadResolutionPriority(1)]
    // public virtual PlOperatorRegistrator Ternary<TResult, T1, T2, T3>(Func<T1, T2, T3, ValueTask<TResult>> op)
    // {
    //     this.Engine.ValidateType<TResult>();
    //     this.Engine.ValidateType<T1>();
    //     this.Engine.ValidateType<T2>();
    //     this.Engine.ValidateType<T3>();
    //     
    //     return this.Ternary(async (t1, t2, t3) => Variant.From(await op(t1.Unpack<T1>(), t2.Unpack<T2>(), t3.Unpack<T3>())));
    // }
    //
    // [OverloadResolutionPriority(1)]
    // public virtual PlOperatorRegistrator Ternary<TResult, T1, T2, T3>(Func<MapperStep, T1, T2, T3, ValueTask<TResult>> op)
    // {
    //     this.Engine.ValidateType<TResult>();
    //     this.Engine.ValidateType<T1>();
    //     this.Engine.ValidateType<T2>();
    //     this.Engine.ValidateType<T3>();
    //     
    //     return this.Ternary(async (step, t1, t2, t3) => Variant.From(await op(step, t1.Unpack<T1>(), t2.Unpack<T2>(), t3.Unpack<T3>())));
    // }
    //
    // public virtual PlOperatorRegistrator Ternary<TResult, T1, T2, T3>(Func<T1, T2, T3, TResult> op)
    // {
    //     this.Engine.ValidateType<TResult>();
    //     this.Engine.ValidateType<T1>();
    //     this.Engine.ValidateType<T2>();
    //     this.Engine.ValidateType<T3>();
    //     
    //     return this.Ternary((scoped in t1, scoped in t2, scoped in t3) => Variant.From(op(t1.Unpack<T1>(), t2.Unpack<T2>(), t3.Unpack<T3>())));
    // }
    //
    // public virtual PlOperatorRegistrator Ternary<TResult, T1, T2, T3>(Func<MapperStep, T1, T2, T3, TResult> op)
    // {
    //     this.Engine.ValidateType<TResult>();
    //     this.Engine.ValidateType<T1>();
    //     this.Engine.ValidateType<T2>();
    //     this.Engine.ValidateType<T3>();
    //     
    //     return this.Ternary((scoped in step, scoped in t1, scoped in t2, scoped in t3) => Variant.From(op(step, t1.Unpack<T1>(), t2.Unpack<T2>(), t3.Unpack<T3>())));
    // }
    //
    // #endregion
    //
    // #region Variadic
    //
    // [OverloadResolutionPriority(2)]
    // public virtual PlOperatorRegistrator Variadic(PlOperatorDelegates.VariadicOp op)
    // {
    //     return this;
    // }
    //
    // [OverloadResolutionPriority(2)]
    // public virtual PlOperatorRegistrator Variadic(PlOperatorDelegates.VariadicOpWithSingleStep op)
    // {
    //     return this;
    // }
    //
    // [OverloadResolutionPriority(2)]
    // public virtual PlOperatorRegistrator Variadic(PlOperatorDelegates.VariadicOpAsync op)
    // {
    //     return this;
    // }
    //
    // [OverloadResolutionPriority(2)]
    // public virtual PlOperatorRegistrator Variadic(PlOperatorDelegates.VariadicOpWithSingleStepAsync op)
    // {
    //     return this;
    // }
    //
    // [OverloadResolutionPriority(1)]
    // public virtual PlOperatorRegistrator Variadic<TResult, T>(Func<ReadOnlyMemory<T>, ValueTask<TResult>> op)
    // {
    //     return this.Variadic<TResult, T>((_, args) => op(args));
    // }
    //
    // [OverloadResolutionPriority(1)]
    // public virtual PlOperatorRegistrator Variadic<TResult, T>(Func<MapperStep, ReadOnlyMemory<T>, ValueTask<TResult>> op)
    // {
    //     this.Engine.ValidateType<TResult>();
    //     this.Engine.ValidateType<T>();
    //     
    //     var pool = this.Engine.GetArrayPool<T>();
    //     
    //     return this.Variadic(async (step, args) =>
    //     {
    //         var argsLength = args.Length;
    //         var buf = pool.Rent(argsLength);
    //
    //         try
    //         {
    //             var argSpan = args.Span;
    //             for (var i = 0; i < argsLength; i++)
    //             {
    //                 buf[i] = argSpan[i].Unpack<T>();
    //             }
    //             
    //             return Variant.From(await op(step, new ReadOnlyMemory<T>(buf, 0, argsLength)));
    //         }
    //         finally
    //         {
    //             pool.Return(buf, true);
    //         }
    //     });
    // }
    //
    // public virtual PlOperatorRegistrator Variadic<TResult, T>(Func<ReadOnlySpan<T>, TResult> op)
    // {
    //     return this.Variadic<TResult, T>((_, args) => op(args));
    // }
    //
    // public virtual PlOperatorRegistrator Variadic<TResult, T>(Func<MapperStep, ReadOnlySpan<T>, TResult> op)
    // {
    //     this.Engine.ValidateType<TResult>();
    //     this.Engine.ValidateType<T>();
    //     
    //     var pool = this.Engine.GetArrayPool<T>();
    //
    //     return this.Variadic((scoped in step, scoped args) =>
    //     {
    //         var argsLength = args.Length;
    //         var buf = pool.Rent(argsLength);
    //         
    //         try
    //         {
    //             for (var i = 0; i < argsLength; i++)
    //             {
    //                 buf[i] = args[i].Unpack<T>();
    //             }
    //             
    //             return Variant.From(op(step, buf.AsSpan(0, argsLength)));
    //         }
    //         finally
    //         {
    //             pool.Return(buf, true);
    //         }
    //     });
    // }
    //
    //
    // #endregion
}