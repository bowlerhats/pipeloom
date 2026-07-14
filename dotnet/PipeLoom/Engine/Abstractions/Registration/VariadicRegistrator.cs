using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Engine.Abstractions.Registration;

public readonly record struct VariadicRegistrator<T>(PlOperatorRegistrator Registrator, HandlerConfig<VariadicHandler> Config)
{
    #region Generic
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Function<TResult>(Func<ReadOnlyMemory<T>, ValueTask<TResult>> op, HandlerConfig<VariadicHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<TResult>();
        this.Registrator.Variadic(
            async args =>
            {
                using var vrecast = new VariantRecaster<T>(resultType.Engine, args.Span);
                return Variant.From(await op(vrecast.Memory), resultType);
            },
            this.Config.Then(h => h.ChangeSignature<T, TResult>()).Then(next)
        );

        return this;
    }
    
    public VariadicRegistrator<T> Function<TResult>(Func<ReadOnlyMemory<T>, TResult> op, HandlerConfig<VariadicHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<TResult>();
        this.Registrator.Variadic(
            (scoped in args) =>
            {
                using var vrecast = new VariantRecaster<T>(resultType.Engine, args.Span);
                return Variant.From(op(vrecast.Memory), resultType);
            },
            this.Config.Then(h => h.ChangeSignature<T, TResult>()).Then(next)
        );

        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Function<TResult>(Func<WeaveStep, ReadOnlyMemory<T>, ValueTask<TResult>> op, HandlerConfig<VariadicHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<TResult>();
        this.Registrator.Variadic(
            async (step, args) =>
            {
                using var vrecast = new VariantRecaster<T>(resultType.Engine, args.Span);
                return Variant.From(await op(step, vrecast.Memory), resultType);
            },
            this.Config.Then(h => h.ChangeSignature<T, TResult>()).Then(next)
        );

        return this;
    }
    
    public VariadicRegistrator<T> Function<TResult>(Func<WeaveStep, ReadOnlyMemory<T>, TResult> op, HandlerConfig<VariadicHandler> next = default)
    {
        var resultType = this.Registrator.Engine.TypeOf<TResult>();
        this.Registrator.Variadic(
            (scoped in step, scoped in args) =>
            {
                using var vrecast = new VariantRecaster<T>(resultType.Engine, args.Span);
                return Variant.From(op(step, vrecast.Memory), resultType);
            },
            this.Config.Then(h => h.ChangeSignature<T, TResult>()).Then(next)
        );

        return this;
    }
    
    #endregion
    
    #region Transfomer
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Transformer<TResult>(Func<ReadOnlyMemory<Many<T>>, ValueTask<Many<TResult>>> op, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.AsVariadic<Many<T>>().Function(
            op,
            this.Config
                .Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>())
                .Then(next)
        );
        
        return this;
    }
    
    public VariadicRegistrator<T> Transformer<TResult>(Func<ReadOnlyMemory<Many<T>>, Many<TResult>> op, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.AsVariadic<Many<T>>().Function(
            op,
            this.Config
                .Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>())
                .Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Transformer<TResult>(Func<WeaveStep, ReadOnlyMemory<Many<T>>, ValueTask<Many<TResult>>> op, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.AsVariadic<Many<T>>().Function(
            op,
            this.Config
                .Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>())
                .Then(next)
        );
        
        return this;
    }
    
    public VariadicRegistrator<T> Transformer<TResult>(Func<WeaveStep, ReadOnlyMemory<Many<T>>, Many<TResult>> op, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.AsVariadic<Many<T>>().Function(
            op,
            this.Config
                .Then(h => h.WithRole(HandlerRole.Transformer).ChangeSignature<Many<T>, Many<TResult>>())
                .Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Reducer
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Reducer<TResult>(Func<ReadOnlyMemory<Many<T>>, ValueTask<TResult>> op, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.AsVariadic<Many<T>>().Function(
            op,
            this.Config
                .Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T>, TResult>())
                .Then(next)
        );
        
        return this;
    }
    
    public VariadicRegistrator<T> Reducer<TResult>(Func<ReadOnlyMemory<Many<T>>, TResult> op, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.AsVariadic<Many<T>>().Function(
            op,
            this.Config
                .Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T>, TResult>())
                .Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Reducer<TResult>(Func<WeaveStep, ReadOnlyMemory<Many<T>>, ValueTask<TResult>> op, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.AsVariadic<Many<T>>().Function(
            op,
            this.Config
                .Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T>, TResult>())
                .Then(next)
        );
        
        return this;
    }
    
    public VariadicRegistrator<T> Reducer<TResult>(Func<WeaveStep, ReadOnlyMemory<Many<T>>, TResult> op, HandlerConfig<VariadicHandler> next = default)
    {
        this.Registrator.AsVariadic<Many<T>>().Function(
            op,
            this.Config
                .Then(h => h.WithRole(HandlerRole.Reducer).ChangeSignature<Many<T>, TResult>())
                .Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Expander
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Expander<TResult>(Func<ReadOnlyMemory<T>, ValueTask<Many<TResult>>> op, HandlerConfig<VariadicHandler> next = default)
    {
        this.Function(
            op,
            this.Config
                .Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T, TResult>())
                .Then(next)
        );
        
        return this;
    }
    
    public VariadicRegistrator<T> Expander<TResult>(Func<ReadOnlyMemory<T>, Many<TResult>> op, HandlerConfig<VariadicHandler> next = default)
    {
        this.Function(
            op,
            this.Config
                .Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T, TResult>())
                .Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Expander<TResult>(Func<WeaveStep, ReadOnlyMemory<T>, ValueTask<Many<TResult>>> op, HandlerConfig<VariadicHandler> next = default)
    {
        this.Function(
            op,
            this.Config
                .Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T, TResult>())
                .Then(next)
        );
        
        return this;
    }
    
    public VariadicRegistrator<T> Expander<TResult>(Func<WeaveStep, ReadOnlyMemory<T>, Many<TResult>> op, HandlerConfig<VariadicHandler> next = default)
    {
        this.Function(
            op,
            this.Config
                .Then(h => h.WithRole(HandlerRole.Expander).ChangeSignature<T, TResult>())
                .Then(next)
        );
        
        return this;
    }
    
    #endregion
    
    #region Bundler
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Bundler<TResult>(Func<ReadOnlyMemory<T>, ValueTask<IBundle<TResult>>> op, HandlerConfig<VariadicHandler> next = default)
    {
        this.Function(
            op,
            this.Config
                .Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<T, IBundle<TResult>>())
                .Then(next)
        );
        
        return this;
    }
    
    public VariadicRegistrator<T> Bundler<TResult>(Func<ReadOnlyMemory<T>, IBundle<TResult>> op, HandlerConfig<VariadicHandler> next = default)
    {
        this.Function(
            op,
            this.Config
                .Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<T, IBundle<TResult>>())
                .Then(next)
        );
        
        return this;
    }
    
    [OverloadResolutionPriority(1)]
    public VariadicRegistrator<T> Bundler<TResult>(Func<WeaveStep, ReadOnlyMemory<T>, ValueTask<IBundle<TResult>>> op, HandlerConfig<VariadicHandler> next = default)
    {
        this.Function(
            op,
            this.Config
                .Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<T, IBundle<TResult>>())
                .Then(next)
        );
        
        return this;
    }
    
    public VariadicRegistrator<T> Bundler<TResult>(Func<WeaveStep, ReadOnlyMemory<T>, IBundle<TResult>> op, HandlerConfig<VariadicHandler> next = default)
    {
        this.Function(
            op,
            this.Config
                .Then(h => h.WithRole(HandlerRole.Bundler).ChangeSignature<T, IBundle<TResult>>())
                .Then(next)
        );
        
        return this;
    }
    
    #endregion
}