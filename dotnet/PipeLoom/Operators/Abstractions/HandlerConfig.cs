using System;
using System.Collections.Immutable;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Operators.Abstractions;

public static class HandlerConfig
{
    public static HandlerConfig<THandler> From<THandler>(Action<THandler> action)
        where THandler: OperatorHandler
    {
        return new HandlerConfig<THandler>([action]);
    }
}

public readonly record struct HandlerConfig<T>(
    ImmutableList<Action<T>>? Actions
)
    where T : OperatorHandler
{
    public HandlerConfig() : this([])
    {
    }
    
    public HandlerConfig(HandlerConfig<T> prev) : this(prev.Actions)
    {
    }
    
    public HandlerConfig<T> Then(HandlerConfig<T> next)
    {
        if (next.Actions is null || next.Actions.IsEmpty)
        {
            return this;
        }
        
        return new HandlerConfig<T>(this.Actions?.AddRange(next.Actions) ?? next.Actions);
    }
    
    public HandlerConfig<T> Then(Action<T>? next)
    {
        return next is null ? this : new HandlerConfig<T>(this.Actions?.Add(next) ?? [ next ]);
    }

    public HandlerConfig<T> Prepend(HandlerConfig<T> previous)
    {
        if (previous.Actions is null || previous.Actions.IsEmpty)
        {
            return this;
        }
        
        return new HandlerConfig<T>(previous.Actions.AddRange(this.Actions ?? []));
    }
    
    public HandlerConfig<T> Prepend(Action<T>? previous)
    {
        return previous is null ? this : new HandlerConfig<T>(this.Actions?.Insert(0, previous) ?? [ previous ]);
    }

    public void Apply(T handler)
    {
        if (this.Actions is null)
            return;
        
        foreach (var action in this.Actions)
        {
            action(handler);
        } 
    }
}
