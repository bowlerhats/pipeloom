using System;
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
    
    protected virtual PlOperatorRegistrator Register(OperatorHandler handler)
    {
        this.OperatorClass.AddHandler(handler);
        
        return this;
    }
    
    #region Nullary

    public NullaryRegistrator AsNullary(Action<NullaryHandler> config) => this.AsNullary(HandlerConfig.From(config));
    public virtual NullaryRegistrator AsNullary(HandlerConfig<NullaryHandler> config = default)
    {
        return new NullaryRegistrator(this, config);
    }
    
    protected virtual PlOperatorRegistrator Nullary(NullaryHandler handler, HandlerConfig<NullaryHandler> config = default)
    {
        config.Apply(handler);

        return this.Register(handler);
    }

    public PlOperatorRegistrator Nullary(PlOperatorDelegates.NullaryFunction op, Action<NullaryHandler> config) => this.Nullary(op, HandlerConfig.From(config));
    public virtual PlOperatorRegistrator Nullary(PlOperatorDelegates.NullaryFunction op, HandlerConfig<NullaryHandler> config = default)
    {
        return this.Nullary(new NullaryHandler(this.OperatorClass, op), config);
    }
    
    public PlOperatorRegistrator Nullary(PlOperatorDelegates.NullaryFunctionWithStep op, Action<NullaryHandler> config) => this.Nullary(op, HandlerConfig.From(config));
    public virtual PlOperatorRegistrator Nullary(PlOperatorDelegates.NullaryFunctionWithStep op, HandlerConfig<NullaryHandler> config = default)
    {
        return this.Nullary(new NullaryHandler(this.OperatorClass, op), config);
    }
    
    public PlOperatorRegistrator Nullary(PlOperatorDelegates.NullaryFunctionAsync op, Action<NullaryHandler> config) => this.Nullary(op, HandlerConfig.From(config));
    public virtual PlOperatorRegistrator Nullary(PlOperatorDelegates.NullaryFunctionAsync op, HandlerConfig<NullaryHandler> config = default)
    {
        return this.Nullary(new NullaryHandler(this.OperatorClass, op), config);
    }
    
    public PlOperatorRegistrator Nullary(PlOperatorDelegates.NullaryFunctionAsyncWithStep op, Action<NullaryHandler> config) => this.Nullary(op, HandlerConfig.From(config));
    public virtual PlOperatorRegistrator Nullary(PlOperatorDelegates.NullaryFunctionAsyncWithStep op, HandlerConfig<NullaryHandler> config = default)
    {
        return this.Nullary(new NullaryHandler(this.OperatorClass, op), config);
    }
    
    #endregion
    
    #region Unary
    
    public UnaryRegistrator<T> AsUnary<T>(Action<UnaryHandler> config) => this.AsUnary<T>(HandlerConfig.From(config));
    public virtual UnaryRegistrator<T> AsUnary<T>(HandlerConfig<UnaryHandler> config = default)
    {
        return new UnaryRegistrator<T>(this, config);
    }
    
    protected virtual PlOperatorRegistrator Unary(UnaryHandler handler, HandlerConfig<UnaryHandler> config = default)
    {
        config.Apply(handler);

        return this.Register(handler);
    }

    public PlOperatorRegistrator Unary(PlOperatorDelegates.UnaryFunction op, Action<UnaryHandler> action) => this.Unary(op, HandlerConfig.From(action));
    public virtual PlOperatorRegistrator Unary(PlOperatorDelegates.UnaryFunction op, HandlerConfig<UnaryHandler> config = default)
    {
        return this.Unary(new UnaryHandler(this.OperatorClass, op), config);
    }
    
    public PlOperatorRegistrator Unary(PlOperatorDelegates.UnaryFunctionWithStep op, Action<UnaryHandler> action) => this.Unary(op, HandlerConfig.From(action));
    public virtual PlOperatorRegistrator Unary(PlOperatorDelegates.UnaryFunctionWithStep op, HandlerConfig<UnaryHandler> config = default)
    {
        return this.Unary(new UnaryHandler(this.OperatorClass, op), config);
    }
    
    public PlOperatorRegistrator Unary(PlOperatorDelegates.UnaryFunctionAsync op, Action<UnaryHandler> action) => this.Unary(op, HandlerConfig.From(action));
    public virtual PlOperatorRegistrator Unary(PlOperatorDelegates.UnaryFunctionAsync op, HandlerConfig<UnaryHandler> config = default)
    {
        return this.Unary(new UnaryHandler(this.OperatorClass, op), config);
    }
    
    public PlOperatorRegistrator Unary(PlOperatorDelegates.UnaryFunctionAsyncWithStep op, Action<UnaryHandler> action) => this.Unary(op, HandlerConfig.From(action));
    public virtual PlOperatorRegistrator Unary(PlOperatorDelegates.UnaryFunctionAsyncWithStep op, HandlerConfig<UnaryHandler> config = default)
    {
        return this.Unary(new UnaryHandler(this.OperatorClass, op), config);
    }
    
    #endregion
    
    #region Binary

    public BinaryRegistrator<T1, T2> AsBinary<T1, T2>(Action<BinaryHandler> config) => this.AsBinary<T1, T2>(HandlerConfig.From(config));
    public virtual BinaryRegistrator<T1, T2> AsBinary<T1, T2>(HandlerConfig<BinaryHandler> config = default)
    {
        return new BinaryRegistrator<T1, T2>(this, config);
    }

    public BinaryRegistrator<TSymmetric, TSymmetric> AsBinary<TSymmetric>(Action<BinaryHandler> config) => this.AsBinary<TSymmetric>(HandlerConfig.From(config));
    public virtual BinaryRegistrator<TSymmetric, TSymmetric> AsBinary<TSymmetric>(HandlerConfig<BinaryHandler> config = default)
    {
        return new BinaryRegistrator<TSymmetric, TSymmetric>(this, config);
    }
    
    protected virtual PlOperatorRegistrator Binary(BinaryHandler handler, HandlerConfig<BinaryHandler> config = default)
    {
        config.Apply(handler);

        return this.Register(handler);
    }

    public PlOperatorRegistrator Binary(PlOperatorDelegates.BinaryFunction op, Action<BinaryHandler> config) => this.Binary(op, HandlerConfig.From(config));
    public virtual PlOperatorRegistrator Binary(PlOperatorDelegates.BinaryFunction op, HandlerConfig<BinaryHandler> config = default)
    {
        return this.Binary(new BinaryHandler(this.OperatorClass, op), config);
    }
    
    public PlOperatorRegistrator Binary(PlOperatorDelegates.BinaryFunctionWithStep op, Action<BinaryHandler> config) => this.Binary(op, HandlerConfig.From(config));
    public virtual PlOperatorRegistrator Binary(PlOperatorDelegates.BinaryFunctionWithStep op, HandlerConfig<BinaryHandler> config = default)
    {
        return this.Binary(new BinaryHandler(this.OperatorClass, op), config);
    }
    
    public PlOperatorRegistrator Binary(PlOperatorDelegates.BinaryFunctionAsync op, Action<BinaryHandler> config) => this.Binary(op, HandlerConfig.From(config));
    public virtual PlOperatorRegistrator Binary(PlOperatorDelegates.BinaryFunctionAsync op, HandlerConfig<BinaryHandler> config = default)
    {
        return this.Binary(new BinaryHandler(this.OperatorClass, op), config);
    }
    
    public PlOperatorRegistrator Binary(PlOperatorDelegates.BinaryFunctionAsyncWithStep op, Action<BinaryHandler> config) => this.Binary(op, HandlerConfig.From(config));
    public virtual PlOperatorRegistrator Binary(PlOperatorDelegates.BinaryFunctionAsyncWithStep op, HandlerConfig<BinaryHandler> config = default)
    {
        return this.Binary(new BinaryHandler(this.OperatorClass, op), config);
    }
    
    #endregion
    
    #region Ternary

    public TernaryRegistrator<T1, T2, T3> AsTernary<T1, T2, T3>(Action<TernaryHandler> config) => this.AsTernary<T1, T2, T3>(HandlerConfig.From(config));
    public virtual TernaryRegistrator<T1, T2, T3> AsTernary<T1, T2, T3>(HandlerConfig<TernaryHandler> config = default)
    {
        return new TernaryRegistrator<T1, T2, T3>(this, config);
    }
    
    protected virtual PlOperatorRegistrator Ternary(TernaryHandler handler, HandlerConfig<TernaryHandler> config = default)
    {
        config.Apply(handler);

        return this.Register(handler);
    }

    public PlOperatorRegistrator Ternary(PlOperatorDelegates.TernaryFunction op, Action<TernaryHandler> config) => this.Ternary(op, HandlerConfig.From(config));
    public virtual PlOperatorRegistrator Ternary(PlOperatorDelegates.TernaryFunction op, HandlerConfig<TernaryHandler> config = default)
    {
        return this.Ternary(new TernaryHandler(this.OperatorClass, op), config);
    }
    
    public PlOperatorRegistrator Ternary(PlOperatorDelegates.TernaryFunctionWithStep op, Action<TernaryHandler> config) => this.Ternary(op, HandlerConfig.From(config));
    public virtual PlOperatorRegistrator Ternary(PlOperatorDelegates.TernaryFunctionWithStep op, HandlerConfig<TernaryHandler> config = default)
    {
        return this.Ternary(new TernaryHandler(this.OperatorClass, op), config);
    }
    
    public PlOperatorRegistrator Ternary(PlOperatorDelegates.TernaryFunctionAsync op, Action<TernaryHandler> config) => this.Ternary(op, HandlerConfig.From(config));
    public virtual PlOperatorRegistrator Ternary(PlOperatorDelegates.TernaryFunctionAsync op, HandlerConfig<TernaryHandler> config = default)
    {
        return this.Ternary(new TernaryHandler(this.OperatorClass, op), config);
    }
    
    public PlOperatorRegistrator Ternary(PlOperatorDelegates.TernaryFunctionAsyncWithStep op, Action<TernaryHandler> config) => this.Ternary(op, HandlerConfig.From(config));
    public virtual PlOperatorRegistrator Ternary(PlOperatorDelegates.TernaryFunctionAsyncWithStep op, HandlerConfig<TernaryHandler> config = default)
    {
        return this.Ternary(new TernaryHandler(this.OperatorClass, op), config);
    }
    
    #endregion
    
    #region Variadic

    public VariadicRegistrator<T> AsVariadic<T>(Action<VariadicHandler> config) => this.AsVariadic<T>(HandlerConfig.From(config));
    public virtual VariadicRegistrator<T> AsVariadic<T>(HandlerConfig<VariadicHandler> config = default)
    {
        return new VariadicRegistrator<T>(this, config);
    }
    
    protected virtual PlOperatorRegistrator Variadic(VariadicHandler handler, HandlerConfig<VariadicHandler> config = default)
    {
        config.Apply(handler);

        return this.Register(handler);
    }

    public PlOperatorRegistrator Variadic(PlOperatorDelegates.VariadicFunction op, Action<VariadicHandler> config) => this.Variadic(op, HandlerConfig.From(config));
    public virtual PlOperatorRegistrator Variadic(PlOperatorDelegates.VariadicFunction op, HandlerConfig<VariadicHandler> config = default)
    {
        return this.Variadic(new VariadicHandler(this.OperatorClass, op), config);
    }
    
    public PlOperatorRegistrator Variadic(PlOperatorDelegates.VariadicFunctionWithStep op, Action<VariadicHandler> config) => this.Variadic(op, HandlerConfig.From(config));
    public virtual PlOperatorRegistrator Variadic(PlOperatorDelegates.VariadicFunctionWithStep op, HandlerConfig<VariadicHandler> config = default)
    {
        return this.Variadic(new VariadicHandler(this.OperatorClass, op), config);
    }
    
    public PlOperatorRegistrator Variadic(PlOperatorDelegates.VariadicFunctionAsync op, Action<VariadicHandler> config) => this.Variadic(op, HandlerConfig.From(config));
    public virtual PlOperatorRegistrator Variadic(PlOperatorDelegates.VariadicFunctionAsync op, HandlerConfig<VariadicHandler> config = default)
    {
        return this.Variadic(new VariadicHandler(this.OperatorClass, op), config);
    }
    
    public PlOperatorRegistrator Variadic(PlOperatorDelegates.VariadicFunctionAsyncWithStep op, Action<VariadicHandler> config) => this.Variadic(op, HandlerConfig.From(config));
    public virtual PlOperatorRegistrator Variadic(PlOperatorDelegates.VariadicFunctionAsyncWithStep op, HandlerConfig<VariadicHandler> config = default)
    {
        return this.Variadic(new VariadicHandler(this.OperatorClass, op), config);
    }
    
    #endregion
}