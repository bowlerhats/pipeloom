using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions.Adapters;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Engine.Abstractions.Registration;

public readonly record struct UnaryRegistrator<T>(PlOperatorRegistrator Registrator, HandlerConfig<UnaryHandler> Config)
{
    private IPipeLoomEngine Engine => this.Registrator.Engine;
    
    #region Generic
    
    [OverloadResolutionPriority(1)]
    public UnaryRegistrator<T> Function<TResult>(Func<T, ValueTask<TResult>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<T, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    public UnaryRegistrator<T> Function<TResult>(Func<T, TResult> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<T, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public UnaryRegistrator<T> Function<TResult>(Func<WeaveStep, T, ValueTask<TResult>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<T, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    public UnaryRegistrator<T> Function<TResult>(Func<WeaveStep, T, TResult> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<T, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    #endregion
    
    #region Mapper
    
    [OverloadResolutionPriority(1)]
    public UnaryRegistrator<T> Mapper<TResult>(Func<T, ValueTask<TResult>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        return this.Function(op, next: next.Prepend(h => h.WithRole(HandlerRole.Mapper)).Then(config));
    }
    
    public UnaryRegistrator<T> Mapper<TResult>(Func<T, TResult> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        return this.Function(op, next: next.Prepend(h => h.WithRole(HandlerRole.Mapper)).Then(config));
    }
    
    [OverloadResolutionPriority(1)]
    public UnaryRegistrator<T> Mapper<TResult>(Func<WeaveStep, T, ValueTask<TResult>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        return this.Function(op, next: next.Prepend(h => h.WithRole(HandlerRole.Mapper)).Then(config));
    }
    
    public UnaryRegistrator<T> Mapper<TResult>(Func<WeaveStep, T, TResult> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        return this.Function(op, next: next.Prepend(h => h.WithRole(HandlerRole.Mapper)).Then(config));
    }
    
    #endregion
    
    #region Transfomer
    
    [OverloadResolutionPriority(1)]
    public UnaryRegistrator<T> Transformer<TResult>(Func<Many<T>, ValueTask<Many<TResult>>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public UnaryRegistrator<T> Transformer<TResult>(Func<Many<T>, Many<TResult>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public UnaryRegistrator<T> Transformer<TResult>(Func<WeaveStep, Many<T>, ValueTask<Many<TResult>>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public UnaryRegistrator<T> Transformer<TResult>(Func<WeaveStep, Many<T>, Many<TResult>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Reducer
    
    [OverloadResolutionPriority(1)]
    public UnaryRegistrator<T> Reducer<TResult>(Func<Many<T>, ValueTask<TResult>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T>, TResult>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public UnaryRegistrator<T> Reducer<TResult>(Func<Many<T>, TResult> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T>, TResult>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public UnaryRegistrator<T> Reducer<TResult>(Func<WeaveStep, Many<T>, ValueTask<TResult>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T>, TResult>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public UnaryRegistrator<T> Reducer<TResult>(Func<WeaveStep, Many<T>, TResult> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T>, TResult>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Expander
    
    [OverloadResolutionPriority(1)]
    public UnaryRegistrator<T> Expander<TResult>(Func<T, ValueTask<Many<TResult>>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public UnaryRegistrator<T> Expander<TResult>(Func<T, Many<TResult>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public UnaryRegistrator<T> Expander<TResult>(Func<WeaveStep, T, ValueTask<Many<TResult>>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public UnaryRegistrator<T> Expander<TResult>(Func<WeaveStep, T, Many<TResult>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Bundler
    
    [OverloadResolutionPriority(1)]
    public UnaryRegistrator<T> Bundler<TResult>(Func<IReadOnlyBundle<T>, ValueTask<IBundle<TResult>>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IReadOnlyBundle<T>, IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public UnaryRegistrator<T> Bundler<TResult>(Func<IReadOnlyBundle<T>, IBundle<TResult>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IReadOnlyBundle<T>, IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public UnaryRegistrator<T> Bundler<TResult>(Func<WeaveStep, IReadOnlyBundle<T>, ValueTask<IBundle<TResult>>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IReadOnlyBundle<T>, IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public UnaryRegistrator<T> Bundler<TResult>(Func<WeaveStep, IReadOnlyBundle<T>, IBundle<TResult>> op, Action<UnaryHandler>? config = null, HandlerConfig<UnaryHandler> next = default)
    {
        this.Registrator.Unary(
            MethodAdapter.Unary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IReadOnlyBundle<T>, IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
}