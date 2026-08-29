using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions.Adapters;
using PipeLoom.Engine.Abstractions.Bundles;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Engine.Abstractions.Registration;

public readonly record struct TernaryRegistrator<T1, T2, T3>(PlOperatorRegistrator Registrator, HandlerConfig<TernaryHandler> Config)
{
    private IPipeLoomEngine Engine => this.Registrator.Engine;
    
    #region Generic
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Function<TResult>(Func<T1, T2, T3, ValueTask<TResult>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<T1, T2, T3, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Function<TResult>(Func<T1, T2, T3, TResult> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<T1, T2, T3, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Function<TResult>(Func<WeaveStep, T1, T2, T3, ValueTask<TResult>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<T1, T2, T3, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Function<TResult>(Func<WeaveStep, T1, T2, T3, TResult> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<T1, T2, T3, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    #endregion
    
    #region Mapper
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Mapper<TResult>(Func<T1, T2, T3, ValueTask<TResult>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        return this.Function(op, next: next.Prepend(h => h.WithRole(HandlerRole.Mapper)).Then(config));
    }
    
    public TernaryRegistrator<T1, T2, T3> Mapper<TResult>(Func<T1, T2, T3, TResult> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        return this.Function(op, next: next.Prepend(h => h.WithRole(HandlerRole.Mapper)).Then(config));
    }
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Mapper<TResult>(Func<WeaveStep, T1, T2, T3, ValueTask<TResult>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        return this.Function(op, next: next.Prepend(h => h.WithRole(HandlerRole.Mapper)).Then(config));
    }
    
    public TernaryRegistrator<T1, T2, T3> Mapper<TResult>(Func<WeaveStep, T1, T2, T3, TResult> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        return this.Function(op, next: next.Prepend(h => h.WithRole(HandlerRole.Mapper)).Then(config));
    }
    
    #endregion
    
    #region Transfomer
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Transformer<TResult>(Func<Many<T1>, T2, T3, ValueTask<Many<TResult>>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T1>, T2, T3, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Transformer<TResult>(Func<Many<T1>, T2, T3, Many<TResult>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T1>, T2, T3, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Transformer<TResult>(Func<WeaveStep, Many<T1>, T2, T3, ValueTask<Many<TResult>>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T1>, T2, T3, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Transformer<TResult>(Func<WeaveStep, Many<T1>, T2, T3, Many<TResult>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T1>, T2, T3, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Reducer
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Reducer<TResult>(Func<Many<T1>, T2, T3, ValueTask<TResult>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T1>, T2, T3, TResult>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Reducer<TResult>(Func<Many<T1>, T2, T3, TResult> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T1>, T2, T3, TResult>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Reducer<TResult>(Func<WeaveStep, Many<T1>, T2, T3, ValueTask<TResult>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T1>, T2, T3, TResult>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Reducer<TResult>(Func<WeaveStep, Many<T1>, T2, T3, TResult> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T1>, T2, T3, TResult>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Expander
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Expander<TResult>(Func<T1, T2, T3, ValueTask<Many<TResult>>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T1, T2, T3, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Expander<TResult>(Func<T1, T2, T3, Many<TResult>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T1, T2, T3, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Expander<TResult>(Func<WeaveStep, T1, T2, T3, ValueTask<Many<TResult>>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T1, T2, T3, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Expander<TResult>(Func<WeaveStep, T1, T2, T3, Many<TResult>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T1, T2, T3, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Bundler
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Bundler<TResult>(Func<IReadOnlyBundle<T1>, T2, T3, ValueTask<IBundle<TResult>>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IReadOnlyBundle<T1>, T2, T3, IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Bundler<TResult>(Func<IReadOnlyBundle<T1>, T2, T3, IBundle<TResult>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IReadOnlyBundle<T1>, T2, T3, IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public TernaryRegistrator<T1, T2, T3> Bundler<TResult>(Func<WeaveStep, IReadOnlyBundle<T1>, T2, T3, ValueTask<IBundle<TResult>>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IReadOnlyBundle<T1>, T2, T3, IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public TernaryRegistrator<T1, T2, T3> Bundler<TResult>(Func<WeaveStep, IReadOnlyBundle<T1>, T2, T3, IBundle<TResult>> op, Action<TernaryHandler>? config = null, HandlerConfig<TernaryHandler> next = default)
    {
        this.Registrator.Ternary(
            MethodAdapter.Ternary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IReadOnlyBundle<T1>, T2, T3, IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
}