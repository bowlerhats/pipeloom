using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine.Abstractions;

public interface IDoubleDispatchCallback
{
    void Dispatch<T, U>(object? state);
}

public interface IDoubleDispatched
{
    void Dispatch<U>(IDoubleDispatchCallback callback, object? state);
    void Dispatch(IDoubleDispatched second, IDoubleDispatchCallback callback, object? state);

    static void Dispatch(Type first, Type second, IDoubleDispatchCallback callback, object? state = null)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        
        if (!DoubleDispatchRegistry.Dispatchers.TryGetValue(first, out var firstDispatched))
            throw new PipeLoomException($"Type not dispatchable: '{first.FullName}'");
        
        if (!DoubleDispatchRegistry.Dispatchers.TryGetValue(second, out var secondDispatched))
            throw new PipeLoomException($"Type not dispatchable: '{second.FullName}'");
        
        secondDispatched.Dispatch(firstDispatched, callback, state);
    }
}

internal static class DoubleDispatchRegistry
{
    public static readonly ConcurrentDictionary<Type, IDoubleDispatched> Dispatchers = [];
}

public sealed class DoubleDispatch<T> : IDoubleDispatched
{
    public void Dispatch(IDoubleDispatched dispatched, IDoubleDispatchCallback callback, object? state)
    {
        dispatched.Dispatch<T>(callback, state);
    }

    public void Dispatch<U>(IDoubleDispatchCallback callback, object? state)
    {
        callback.Dispatch<T, U>(state);
    }
    
    // Work hard to avoid elision
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void Register()
    {
        DoubleDispatchRegistry.Dispatchers[typeof(T)] = new DoubleDispatch<T>();
    }
}