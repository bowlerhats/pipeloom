using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions.Adapters;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Engine.Abstractions.Registration;

public readonly record struct NullaryRegistrator(PlOperatorRegistrator Registrator, HandlerConfig<NullaryHandler> Config)
{
    private IPipeLoomEngine Engine => this.Registrator.Engine;
    
    #region Generic
    
    [OverloadResolutionPriority(1)]
    public NullaryRegistrator Function<TResult>(Func<ValueTask<TResult>> op, Action<NullaryHandler>? config = null, HandlerConfig<NullaryHandler> next = default)
    {
        this.Registrator.Nullary(
            MethodAdapter.Nullary(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    public NullaryRegistrator Function<TResult>(Func<TResult> op, Action<NullaryHandler>? config = null, HandlerConfig<NullaryHandler> next = default)
    {
        this.Registrator.Nullary(
            MethodAdapter.Nullary(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public NullaryRegistrator Function<TResult>(Func<WeaveStep, ValueTask<TResult>> op, Action<NullaryHandler>? config = null, HandlerConfig<NullaryHandler> next = default)
    {
        this.Registrator.Nullary(
            MethodAdapter.Nullary(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    public NullaryRegistrator Function<TResult>(Func<WeaveStep, TResult> op, Action<NullaryHandler>? config = null, HandlerConfig<NullaryHandler> next = default)
    {
        this.Registrator.Nullary(
            MethodAdapter.Nullary(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    #endregion
    
    #region Expander
    
    [OverloadResolutionPriority(1)]
    public NullaryRegistrator Expander<TResult>(Func<ValueTask<Many<TResult>>> op, Action<NullaryHandler>? config = null, HandlerConfig<NullaryHandler> next = default)
    {
        this.Registrator.Nullary(
            MethodAdapter.Nullary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public NullaryRegistrator Expander<TResult>(Func<Many<TResult>> op, Action<NullaryHandler>? config = null, HandlerConfig<NullaryHandler> next = default)
    {
        this.Registrator.Nullary(
            MethodAdapter.Nullary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public NullaryRegistrator Expander<TResult>(Func<WeaveStep, ValueTask<Many<TResult>>> op, Action<NullaryHandler>? config = null, HandlerConfig<NullaryHandler> next = default)
    {
        this.Registrator.Nullary(
            MethodAdapter.Nullary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public NullaryRegistrator Expander<TResult>(Func<WeaveStep, Many<TResult>> op, Action<NullaryHandler>? config = null, HandlerConfig<NullaryHandler> next = default)
    {
        this.Registrator.Nullary(
            MethodAdapter.Nullary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Bundler
    
    [OverloadResolutionPriority(1)]
    public NullaryRegistrator Bundler<TResult>(Func<ValueTask<IBundle<TResult>>> op, Action<NullaryHandler>? config = null, HandlerConfig<NullaryHandler> next = default)
    {
        this.Registrator.Nullary(
            MethodAdapter.Nullary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public NullaryRegistrator Bundler<TResult>(Func<IBundle<TResult>> op, Action<NullaryHandler>? config = null, HandlerConfig<NullaryHandler> next = default)
    {
        this.Registrator.Nullary(
            MethodAdapter.Nullary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public NullaryRegistrator Bundler<TResult>(Func<WeaveStep, ValueTask<IBundle<TResult>>> op, Action<NullaryHandler>? config = null, HandlerConfig<NullaryHandler> next = default)
    {
        this.Registrator.Nullary(
            MethodAdapter.Nullary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public NullaryRegistrator Bundler<TResult>(Func<WeaveStep, IBundle<TResult>> op, Action<NullaryHandler>? config = null, HandlerConfig<NullaryHandler> next = default)
    {
        this.Registrator.Nullary(
            MethodAdapter.Nullary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
}