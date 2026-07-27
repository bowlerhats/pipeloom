using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions.Adapters;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Engine.Abstractions.Registration;

public readonly record struct BinaryRegistrator<T1, T2>(PlOperatorRegistrator Registrator, HandlerConfig<BinaryHandler> Config)
{
    private IPipeLoomEngine Engine => this.Registrator.Engine;
    
    #region Generic
    
    [OverloadResolutionPriority(1)]
    public BinaryRegistrator<T1, T2> Function<TResult>(Func<T1, T2, ValueTask<TResult>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<T1, T2, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    public BinaryRegistrator<T1, T2> Function<TResult>(Func<T1, T2, TResult> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<T1, T2, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public BinaryRegistrator<T1, T2> Function<TResult>(Func<WeaveStep, T1, T2, ValueTask<TResult>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<T1, T2, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    public BinaryRegistrator<T1, T2> Function<TResult>(Func<WeaveStep, T1, T2, TResult> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.ChangeSignature<T1, T2, TResult>()).Then(config).Then(next)
        );

        return this;
    }
    
    #endregion
    
    #region Mapper
    
    [OverloadResolutionPriority(1)]
    public BinaryRegistrator<T1, T2> Mapper<TResult>(Func<T1, T2, ValueTask<TResult>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        return this.Function(op, next: next.Prepend(h => h.WithRole(HandlerRole.Mapper)).Then(config));
    }
    
    public BinaryRegistrator<T1, T2> Mapper<TResult>(Func<T1, T2, TResult> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        return this.Function(op, next: next.Prepend(h => h.WithRole(HandlerRole.Mapper)).Then(config));
    }
    
    [OverloadResolutionPriority(1)]
    public BinaryRegistrator<T1, T2> Mapper<TResult>(Func<WeaveStep, T1, T2, ValueTask<TResult>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        return this.Function(op, next: next.Prepend(h => h.WithRole(HandlerRole.Mapper)).Then(config));
    }
    
    public BinaryRegistrator<T1, T2> Mapper<TResult>(Func<WeaveStep, T1, T2, TResult> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        return this.Function(op, next: next.Prepend(h => h.WithRole(HandlerRole.Mapper)).Then(config));
    }
    
    #endregion
    
    #region Transfomer
    
    [OverloadResolutionPriority(1)]
    public BinaryRegistrator<T1, T2> Transformer<TResult>(Func<Many<T1>, T2, ValueTask<Many<TResult>>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T1>, T2, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public BinaryRegistrator<T1, T2> Transformer<TResult>(Func<Many<T1>, T2, Many<TResult>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T1>, T2, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public BinaryRegistrator<T1, T2> Transformer<TResult>(Func<WeaveStep, Many<T1>, T2, ValueTask<Many<TResult>>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T1>, T2, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public BinaryRegistrator<T1, T2> Transformer<TResult>(Func<WeaveStep, Many<T1>, T2, Many<TResult>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T1>, T2, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Reducer
    
    [OverloadResolutionPriority(1)]
    public BinaryRegistrator<T1, T2> Reducer<TResult>(Func<Many<T1>, T2, ValueTask<TResult>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T1>, T2, TResult>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public BinaryRegistrator<T1, T2> Reducer<TResult>(Func<Many<T1>, T2, TResult> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T1>, T2, TResult>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public BinaryRegistrator<T1, T2> Reducer<TResult>(Func<WeaveStep, Many<T1>, T2, ValueTask<TResult>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T1>, T2, TResult>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public BinaryRegistrator<T1, T2> Reducer<TResult>(Func<WeaveStep, Many<T1>, T2, TResult> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T1>, T2, TResult>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Expander
    
    [OverloadResolutionPriority(1)]
    public BinaryRegistrator<T1, T2> Expander<TResult>(Func<T1, T2, ValueTask<Many<TResult>>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T1, T2, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public BinaryRegistrator<T1, T2> Expander<TResult>(Func<T1, T2, Many<TResult>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T1, T2, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public BinaryRegistrator<T1, T2> Expander<TResult>(Func<WeaveStep, T1, T2, ValueTask<Many<TResult>>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T1, T2, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public BinaryRegistrator<T1, T2> Expander<TResult>(Func<WeaveStep, T1, T2, Many<TResult>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T1, T2, Many<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Bundler
    
    [OverloadResolutionPriority(1)]
    public BinaryRegistrator<T1, T2> Bundler<TResult>(Func<T1, T2, ValueTask<IBundle<TResult>>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<T1, T2, IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public BinaryRegistrator<T1, T2> Bundler<TResult>(Func<T1, T2, IBundle<TResult>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<T1, T2, IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public BinaryRegistrator<T1, T2> Bundler<TResult>(Func<WeaveStep, T1, T2, ValueTask<IBundle<TResult>>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<T1, T2, IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    public BinaryRegistrator<T1, T2> Bundler<TResult>(Func<WeaveStep, T1, T2, IBundle<TResult>> op, Action<BinaryHandler>? config = null, HandlerConfig<BinaryHandler> next = default)
    {
        this.Registrator.Binary(
            MethodAdapter.Binary(this.Engine, op),
            this.Config.Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<T1, T2, IBundle<TResult>>()).Then(config).Then(next)
        );
        
        return this;
    }
    
    #endregion
}