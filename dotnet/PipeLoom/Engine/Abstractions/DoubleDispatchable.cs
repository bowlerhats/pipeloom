using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine.Abstractions;

public interface IDoubleDispatchCallback<TResult>
{
    TResult Dispatch<T, U>(object? state);
}

public interface IDoubleDispatched
{
    TResult Dispatch<U, TResult>(IDoubleDispatchCallback<TResult> callback, object? state);
    TResult Dispatch<TResult>(IDoubleDispatched second, IDoubleDispatchCallback<TResult> callback, object? state);

    static TResult Dispatch<TResult>(Type first, Type second, IDoubleDispatchCallback<TResult> callback, object? state = null)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        
        if (!DoubleDispatchRegistry.Dispatchers.TryGetValue(first, out var firstDispatched))
            throw new PipeLoomException($"Type not dispatchable: '{first.FullName}'");
        
        if (!DoubleDispatchRegistry.Dispatchers.TryGetValue(second, out var secondDispatched))
            throw new PipeLoomException($"Type not dispatchable: '{second.FullName}'");
        
        return secondDispatched.Dispatch<TResult>(firstDispatched, callback, state);
    }
}

internal static class DoubleDispatchRegistry
{
    public static readonly ConcurrentDictionary<Type, IDoubleDispatched> Dispatchers = [];
}

public sealed class DoubleDispatch<T> : IDoubleDispatched
{
    public TResult Dispatch<TResult>(IDoubleDispatched dispatched, IDoubleDispatchCallback<TResult> callback, object? state)
    {
        return dispatched.Dispatch<T, TResult>(callback, state);
    }

    public TResult Dispatch<U, TResult>(IDoubleDispatchCallback<TResult> callback, object? state)
    {
        return callback.Dispatch<T, U>(state);
    }
    
    // Work hard to avoid elision
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void Register()
    {
        DoubleDispatchRegistry.Dispatchers[typeof(T)] = new DoubleDispatch<T>();
    }
}