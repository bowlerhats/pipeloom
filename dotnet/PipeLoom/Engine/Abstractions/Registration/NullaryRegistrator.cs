using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Engine.Abstractions.Registration;

public readonly record struct NullaryRegistrator(PlOperatorRegistrator Registrator, HandlerConfig<NullaryHandler> Config)
{
    #region Generic
    
    [OverloadResolutionPriority(1)]
    public NullaryRegistrator Function<TResult>(Func<ValueTask<TResult>> op, HandlerConfig<NullaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<TResult>();
        this.Registrator.Nullary(
            async () => Variant.From(await op(), resultType),
            this.Config.Then(h => h.ChangeSignature<TResult>()).Then(next)
        );

        return this;
    }
    
    public NullaryRegistrator Function<TResult>(Func<TResult> op, HandlerConfig<NullaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<TResult>();
        this.Registrator.Nullary(
            () => Variant.From(op(), resultType),
            this.Config.Then(h => h.ChangeSignature<TResult>()).Then(next)
        );

        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public NullaryRegistrator Function<TResult>(Func<WeaveStep, ValueTask<TResult>> op, HandlerConfig<NullaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<TResult>();
        this.Registrator.Nullary(
            async step => Variant.From(await op(step), resultType),
            this.Config.Then(h => h.ChangeSignature<TResult>()).Then(next)
        );

        return this;
    }
    
    public NullaryRegistrator Function<TResult>(Func<WeaveStep, TResult> op, HandlerConfig<NullaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<TResult>();
        this.Registrator.Nullary(
            (scoped in step) => Variant.From(op(step), resultType),
            this.Config.Then(h => h.ChangeSignature<TResult>()).Then(next)
        );

        return this;
    }
    
    #endregion
    
    #region Expander
    
    [OverloadResolutionPriority(1)]
    public NullaryRegistrator Expander<TResult>(Func<ValueTask<Many<TResult>>> op, HandlerConfig<NullaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<Many<TResult>>();
        this.Registrator.Nullary(
            async () => Variant.From(await op(), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<Many<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    public NullaryRegistrator Expander<TResult>(Func<Many<TResult>> op, HandlerConfig<NullaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<Many<TResult>>();
        this.Registrator.Nullary(
            () => Variant.From(op(), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<Many<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public NullaryRegistrator Expander<TResult>(Func<WeaveStep, ValueTask<Many<TResult>>> op, HandlerConfig<NullaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<Many<TResult>>();
        this.Registrator.Nullary(
            async step => Variant.From(await op(step), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<Many<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    public NullaryRegistrator Expander<TResult>(Func<WeaveStep, Many<TResult>> op, HandlerConfig<NullaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<Many<TResult>>();
        this.Registrator.Nullary(
            (scoped in step) => Variant.From(op(step), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<Many<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Bundler
    
    [OverloadResolutionPriority(1)]
    public NullaryRegistrator Bundler<TResult>(Func<ValueTask<IBundle<TResult>>> op, HandlerConfig<NullaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<IBundle<TResult>>();
        this.Registrator.Nullary(
            async () => Variant.From(await op(), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IBundle<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    public NullaryRegistrator Bundler<TResult>(Func<IBundle<TResult>> op, HandlerConfig<NullaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<IBundle<TResult>>();
        this.Registrator.Nullary(
            () => Variant.From(op(), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IBundle<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public NullaryRegistrator Bundler<TResult>(Func<WeaveStep, ValueTask<IBundle<TResult>>> op, HandlerConfig<NullaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<IBundle<TResult>>();
        this.Registrator.Nullary(
            async step => Variant.From(await op(step), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IBundle<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    public NullaryRegistrator Bundler<TResult>(Func<WeaveStep, IBundle<TResult>> op, HandlerConfig<NullaryHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<IBundle<TResult>>();
        this.Registrator.Nullary(
            (scoped in step) => Variant.From(op(step), resultType),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IBundle<TResult>>()).Then(next)
        );
        
        return this;
    }
    
    #endregion
}