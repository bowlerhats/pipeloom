using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions.Adapters;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Engine.Abstractions.Registration;

public readonly record struct VariadicRegistrator<T>(PlOperatorRegistrator Registrator, HandlerConfig<VariadicHandler> Config)
{
    private IPipeLoomEngine Engine => this.Registrator.Engine;
    
    #region Generic
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Function<TResult>(Func<ReadOnlyMemory<T>, ValueTask<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<T, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    public VariadicRegistrator<T> Function<TResult>(Func<ReadOnlyMemory<T>, TResult> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<T, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Function<TResult>(Func<WeaveStep, ReadOnlyMemory<T>, ValueTask<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<T, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    public VariadicRegistrator<T> Function<TResult>(Func<WeaveStep, ReadOnlyMemory<T>, TResult> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<T, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    #endregion
    
    #region Transfomer
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Transformer<TResult>(Func<ReadOnlyMemory<Many<T>>, ValueTask<Many<TResult>>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>()).Then(config).Then(next)
        );
        this.Registrator.AsVariadic<Many<T>>(this.Config).Function(
            op,
            next: next
                .Prepend(config)
                .Prepend(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>())
        );
        
        return this;
    }
    
    public VariadicRegistrator<T> Transformer<TResult>(Func<ReadOnlyMemory<Many<T>>, Many<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.AsVariadic<Many<T>>(this.Config).Function(
            op,
            next: next
                .Prepend(config)
                .Prepend(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>())
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Transformer<TResult>(Func<WeaveStep, ReadOnlyMemory<Many<T>>, ValueTask<Many<TResult>>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.AsVariadic<Many<T>>(this.Config).Function(
            op,
            next: next
                .Prepend(config)
                .Prepend(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>())
        );
        
        return this;
    }
    
    public VariadicRegistrator<T> Transformer<TResult>(Func<WeaveStep, ReadOnlyMemory<Many<T>>, Many<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.AsVariadic<Many<T>>(this.Config).Function(
            op,
            next: next
                .Prepend(config)
                .Prepend(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>())
        );
        
        return this;
    }
    
    #endregion
    
    #region Reducer
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Reducer<TResult>(Func<ReadOnlyMemory<Many<T>>, ValueTask<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.AsVariadic<Many<T>>(this.Config).Function(
            op,
            next: next
                .Prepend(config)
                .Prepend(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>())
        );
        
        return this;
    }
    
    public VariadicRegistrator<T> Reducer<TResult>(Func<ReadOnlyMemory<Many<T>>, TResult> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.AsVariadic<Many<T>>(this.Config).Function(
            op,
            next: next
                .Prepend(config)
                .Prepend(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>())
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Reducer<TResult>(Func<WeaveStep, ReadOnlyMemory<Many<T>>, ValueTask<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.AsVariadic<Many<T>>(this.Config).Function(
            op,
            next: next
                .Prepend(config)
                .Prepend(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>())
        );
        
        return this;
    }
    
    public VariadicRegistrator<T> Reducer<TResult>(Func<WeaveStep, ReadOnlyMemory<Many<T>>, TResult> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.AsVariadic<Many<T>>(this.Config).Function(
            op,
            next: next
                .Prepend(config)
                .Prepend(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>())
        );
        
        return this;
    }
    
    #endregion
    
    #region Expander
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Expander<TResult>(Func<ReadOnlyMemory<T>, ValueTask<Many<TResult>>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Function(
            op,
            next: next
                .Prepend(config)
                .Prepend(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T, TResult>())
        );
        
        return this;
    }
    
    public VariadicRegistrator<T> Expander<TResult>(Func<ReadOnlyMemory<T>, Many<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Function(
            op,
            next: next
                .Prepend(config)
                .Prepend(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T, TResult>())
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Expander<TResult>(Func<WeaveStep, ReadOnlyMemory<T>, ValueTask<Many<TResult>>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Function(
            op,
            next: next
                .Prepend(config)
                .Prepend(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T, TResult>())
        );
        
        return this;
    }
    
    public VariadicRegistrator<T> Expander<TResult>(Func<WeaveStep, ReadOnlyMemory<T>, Many<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Function(
            op,
            next: next
                .Prepend(config)
                .Prepend(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T, TResult>())
        );
        
        return this;
    }
    
    #endregion
    
    #region Bundler
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Bundler<TResult>(Func<ReadOnlyMemory<T>, ValueTask<IBundle<TResult>>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Function(
            op,
            next: next
                .Prepend(config)
                .Prepend(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<T, IBundle<TResult>>())
        );
        
        return this;
    }
    
    public VariadicRegistrator<T> Bundler<TResult>(Func<ReadOnlyMemory<T>, IBundle<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Function(
            op,
            next: next
                .Prepend(config)
                .Prepend(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<T, IBundle<TResult>>())
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Bundler<TResult>(Func<WeaveStep, ReadOnlyMemory<T>, ValueTask<IBundle<TResult>>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Function(
            op,
            next: next
                .Prepend(config)
                .Prepend(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<T, IBundle<TResult>>())
        );
        
        return this;
    }
    
    public VariadicRegistrator<T> Bundler<TResult>(Func<WeaveStep, ReadOnlyMemory<T>, IBundle<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Function(
            op,
            next: next
                .Prepend(config)
                .Prepend(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<T, IBundle<TResult>>())
        );
        
        return this;
    }
    
    #endregion
}