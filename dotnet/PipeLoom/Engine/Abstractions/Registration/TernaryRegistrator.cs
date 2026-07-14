using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Engine.Abstractions.Registration;

public readonly record struct TernaryRegistrator<T1, T2, T3>(PlOperatorRegistrator Registrator, HandlerConfig<TernaryHandler> Config)
{
    #region Generic
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Function<TResult>(Func<T1, T2, T3, ValueTask<TResult>> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<TResult>();
        this.Registrator.Ternary(
            async (t1, t2, t3) => Variant.From(await op(t1.Unpack<T1>(reinterpret: true), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.ChangeSignature<T1, T2, T3, TResult>()).Then(next)
        );

        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Function<TResult>(Func<T1, T2, T3, TResult> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<TResult>();
        this.Registrator.Ternary(
            (scoped in t1, scoped in t2, scoped in t3) => Variant.From(op(t1.Unpack<T1>(reinterpret: true), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.ChangeSignature<T1, T2, T3, TResult>()).Then(next)
        );

        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Function<TResult>(Func<WeaveStep, T1, T2, T3, ValueTask<TResult>> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<TResult>();
        this.Registrator.Ternary(
            async (step, t1, t2, t3) => Variant.From(await op(step, t1.Unpack<T1>(reinterpret: true), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.ChangeSignature<T1, T2, T3, TResult>()).Then(next)
        );

        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Function<TResult>(Func<WeaveStep, T1, T2, T3, TResult> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<TResult>();
        this.Registrator.Ternary(
            (scoped in step, scoped in t1, scoped in t2, scoped in t3) => Variant.From(op(step, t1.Unpack<T1>(reinterpret: true), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.ChangeSignature<T1, T2, T3, TResult>()).Then(next)
        );

        return this;
    }
    
    #endregion
    
    #region Mapper
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Mapper<TResult>(Func<T1, T2, T3, ValueTask<TResult>> op, HandlerConfig<TernaryHandler> next = default)
    {
        return this.Function(op, next.After(h => h.WithRole(HandlerRole.Mapper)));
    }
    
    public TernaryRegistrator<T1, T2, T3> Mapper<TResult>(Func<T1, T2, T3, TResult> op, HandlerConfig<TernaryHandler> next = default)
    {
        return this.Function(op, next.After(h => h.WithRole(HandlerRole.Mapper)));
    }
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Mapper<TResult>(Func<WeaveStep, T1, T2, T3, ValueTask<TResult>> op, HandlerConfig<TernaryHandler> next = default)
    {
        return this.Function(op, next.After(h => h.WithRole(HandlerRole.Mapper)));
    }
    
    public TernaryRegistrator<T1, T2, T3> Mapper<TResult>(Func<WeaveStep, T1, T2, T3, TResult> op, HandlerConfig<TernaryHandler> next = default)
    {
        return this.Function(op, next.After(h => h.WithRole(HandlerRole.Mapper)));
    }
    
    #endregion
    
    #region Transfomer
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Transformer<TResult>(Func<Many<T1>, T2, T3, ValueTask<Many<TResult>>> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<Many<TResult>>();
        this.Registrator.Ternary(
            async (t1, t2, t3) => Variant.From(await op(t1.Unpack<Many<T1>>(), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T1>, T2, T3, Many<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Transformer<TResult>(Func<Many<T1>, T2, T3, Many<TResult>> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<Many<TResult>>();
        this.Registrator.Ternary(
            (scoped in t1, scoped in t2, scoped in t3) => Variant.From(op(t1.Unpack<Many<T1>>(), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T1>, T2, T3, Many<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Transformer<TResult>(Func<WeaveStep, Many<T1>, T2, T3, ValueTask<Many<TResult>>> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<Many<TResult>>();
        this.Registrator.Ternary(
            async (step, t1, t2, t3) => Variant.From(await op(step, t1.Unpack<Many<T1>>(), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T1>, T2, T3, Many<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Transformer<TResult>(Func<WeaveStep, Many<T1>, T2, T3, Many<TResult>> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<Many<TResult>>();
        this.Registrator.Ternary(
            (scoped in step, scoped in t1, scoped in t2, scoped in t3) => Variant.From(op(step, t1.Unpack<Many<T1>>(), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T1>, T2, T3, Many<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Reducer
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Reducer<TResult>(Func<Many<T1>, T2, T3, ValueTask<TResult>> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<TResult>();
        this.Registrator.Ternary(
            async (t1, t2, t3) => Variant.From(await op(t1.Unpack<Many<T1>>(), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T1>, T2, T3, TResult>()).Then(next)
        );
        
        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Reducer<TResult>(Func<Many<T1>, T2, T3, TResult> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<TResult>();
        this.Registrator.Ternary(
            (scoped in t1, scoped in t2, scoped in t3) => Variant.From(op(t1.Unpack<Many<T1>>(), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T1>, T2, T3, TResult>()).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Reducer<TResult>(Func<WeaveStep, Many<T1>, T2, T3, ValueTask<TResult>> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<TResult>();
        this.Registrator.Ternary(
            async (step, t1, t2, t3) => Variant.From(await op(step, t1.Unpack<Many<T1>>(), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T1>, T2, T3, TResult>()).Then(next)
        );
        
        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Reducer<TResult>(Func<WeaveStep, Many<T1>, T2, T3, TResult> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<TResult>();
        this.Registrator.Ternary(
            (scoped in step, scoped in t1, scoped in t2, scoped in t3) => Variant.From(op(step, t1.Unpack<Many<T1>>(), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T1>, T2, T3, TResult>()).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Expander
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Expander<TResult>(Func<T1, T2, T3, ValueTask<Many<TResult>>> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<Many<TResult>>();
        this.Registrator.Ternary(
            async (t1, t2, t3) => Variant.From(await op(t1.Unpack<T1>(reinterpret: true), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T1, T2, T3, Many<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Expander<TResult>(Func<T1, T2, T3, Many<TResult>> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<Many<TResult>>();
        this.Registrator.Ternary(
            (scoped in t1, scoped in t2, scoped in t3) => Variant.From(op(t1.Unpack<T1>(reinterpret: true), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T1, T2, T3, Many<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Expander<TResult>(Func<WeaveStep, T1, T2, T3, ValueTask<Many<TResult>>> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<Many<TResult>>();
        this.Registrator.Ternary(
            async (step, t1, t2, t3) => Variant.From(await op(step, t1.Unpack<T1>(reinterpret: true), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T1, T2, T3, Many<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Expander<TResult>(Func<WeaveStep, T1, T2, T3, Many<TResult>> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<Many<TResult>>();
        this.Registrator.Ternary(
            (scoped in step, scoped in t1, scoped in t2, scoped in t3) => Variant.From(op(step, t1.Unpack<T1>(reinterpret: true), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T1, T2, T3, Many<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Bundler
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Bundler<TResult>(Func<T1, T2, T3, ValueTask<IBundle<TResult>>> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<IBundle<TResult>>();
        this.Registrator.Ternary(
            async (t1, t2, t3) => Variant.From(await op(t1.Unpack<T1>(reinterpret: true), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<T1, T2, T3, IBundle<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Bundler<TResult>(Func<T1, T2, T3, IBundle<TResult>> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<IBundle<TResult>>();
        this.Registrator.Ternary(
            (scoped in t1, scoped in t2, scoped in t3) => Variant.From(op(t1.Unpack<T1>(reinterpret: true), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<T1, T2, T3, IBundle<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Bundler<TResult>(Func<WeaveStep, T1, T2, T3, ValueTask<IBundle<TResult>>> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<IBundle<TResult>>();
        this.Registrator.Ternary(
            async (step, t1, t2, t3) => Variant.From(await op(step, t1.Unpack<T1>(reinterpret: true), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<T1, T2, T3, IBundle<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Bundler<TResult>(Func<WeaveStep, T1, T2, T3, IBundle<TResult>> op, HandlerConfig<TernaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<IBundle<TResult>>();
        this.Registrator.Ternary(
            (scoped in step, scoped in t1, scoped in t2, scoped in t3) => Variant.From(op(step, t1.Unpack<T1>(reinterpret: true), t2.Unpack<T2>(reinterpret: true), t3.Unpack<T3>(reinterpret: true)), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<T1, T2, T3, IBundle<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    #endregion
}