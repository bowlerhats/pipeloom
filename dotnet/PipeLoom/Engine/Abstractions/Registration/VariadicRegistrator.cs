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
}