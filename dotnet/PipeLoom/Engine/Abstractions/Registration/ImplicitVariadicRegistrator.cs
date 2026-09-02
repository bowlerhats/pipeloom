using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions.Adapters;
using PipeLoom.Engine.Abstractions.Bundles;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Engine.Abstractions.Registration;

public readonly record struct ImplicitVariadicRegistrator<TImplicit, TVariadic>(PlOperatorRegistrator Registrator, HandlerConfig<VariadicHandler> Config)
{
    private IPipeLoomEngine Engine => this.Registrator.Engine;
    
    #region Generic
    
    [OverloadResolutionPriority(1)]
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Function<TResult>(Func<TImplicit, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<TImplicit, TVariadic, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Function<TResult>(Func<TImplicit, ReadOnlyMemory<TVariadic>, TResult> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<TImplicit, TVariadic, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Function<TResult>(Func<WeaveStep, TImplicit, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<TImplicit, TVariadic, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Function<TResult>(Func<WeaveStep, TImplicit, ReadOnlyMemory<TVariadic>, TResult> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<TImplicit, TVariadic, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    #endregion
    
    #region Mapper
    
    [OverloadResolutionPriority(1)]
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Mapper<TResult>(Func<TImplicit, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        return this.Function(op, next: next.Prepend(h => h.WithRole(HandlerRole.Mapper)).Then(config));
    }
    
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Mapper<TResult>(Func<TImplicit, ReadOnlyMemory<TVariadic>, TResult> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        return this.Function(op, next: next.Prepend(h => h.WithRole(HandlerRole.Mapper)).Then(config));
    }
    
    [OverloadResolutionPriority(1)]
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Mapper<TResult>(Func<WeaveStep, TImplicit, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        return this.Function(op, next: next.Prepend(h => h.WithRole(HandlerRole.Mapper)).Then(config));
    }
    
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Mapper<TResult>(Func<WeaveStep, TImplicit, ReadOnlyMemory<TVariadic>, TResult> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        return this.Function(op, next: next.Prepend(h => h.WithRole(HandlerRole.Mapper)).Then(config));
    }
    
    #endregion
    
    #region Transfomer
    
    [OverloadResolutionPriority(1)]
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Transformer<TResult>(Func<Many<TImplicit>, ReadOnlyMemory<TVariadic>, ValueTask<Many<TResult>>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<TImplicit>, TVariadic, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Transformer<TResult>(Func<Many<TImplicit>, ReadOnlyMemory<TVariadic>, Many<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<TImplicit>, TVariadic, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Transformer<TResult>(Func<WeaveStep, Many<TImplicit>, ReadOnlyMemory<TVariadic>, ValueTask<Many<TResult>>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<TImplicit>, TVariadic, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Transformer<TResult>(Func<WeaveStep, Many<TImplicit>, ReadOnlyMemory<TVariadic>, Many<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<TImplicit>, TVariadic, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Reducer
    
    [OverloadResolutionPriority(1)]
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Reducer<TResult>(Func<Many<TImplicit>, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<TImplicit>, TVariadic, TResult>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Reducer<TResult>(Func<Many<TImplicit>, ReadOnlyMemory<TVariadic>, TResult> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<TImplicit>, TVariadic, TResult>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Reducer<TResult>(Func<WeaveStep, Many<TImplicit>, ReadOnlyMemory<TVariadic>, ValueTask<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<TImplicit>, TVariadic, TResult>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Reducer<TResult>(Func<WeaveStep, Many<TImplicit>, ReadOnlyMemory<TVariadic>, TResult> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<TImplicit>, TVariadic, TResult>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Expander
    
    [OverloadResolutionPriority(1)]
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Expander<TResult>(Func<TImplicit, ReadOnlyMemory<TVariadic>, ValueTask<Many<TResult>>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<TImplicit, TVariadic, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Expander<TResult>(Func<TImplicit, ReadOnlyMemory<TVariadic>, Many<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<TImplicit, TVariadic, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Expander<TResult>(Func<WeaveStep, TImplicit, ReadOnlyMemory<TVariadic>, ValueTask<Many<TResult>>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<TImplicit, TVariadic, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Expander<TResult>(Func<WeaveStep, TImplicit, ReadOnlyMemory<TVariadic>, Many<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<TImplicit, TVariadic, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Bundler
    
    [OverloadResolutionPriority(1)]
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Bundler<TResult>(Func<IReadOnlyBundle<TImplicit>, ReadOnlyMemory<TVariadic>, ValueTask<IBundle<TResult>>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IReadOnlyBundle<TImplicit>, TVariadic, IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Bundler<TResult>(Func<IReadOnlyBundle<TImplicit>, ReadOnlyMemory<TVariadic>, IBundle<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IReadOnlyBundle<TImplicit>, TVariadic, IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Bundler<TResult>(Func<WeaveStep, IReadOnlyBundle<TImplicit>, ReadOnlyMemory<TVariadic>, ValueTask<IBundle<TResult>>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IReadOnlyBundle<TImplicit>, TVariadic, IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public ImplicitVariadicRegistrator<TImplicit, TVariadic> Bundler<TResult>(Func<WeaveStep, IReadOnlyBundle<TImplicit>, ReadOnlyMemory<TVariadic>, IBundle<TResult>> op, Action<VariadicHandler>? config = null, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.Variadic(
            MethodAdapter.Variadic(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IReadOnlyBundle<TImplicit>, TVariadic, IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
}