using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PipeLoom.Engine;

namespace PipeLoom.Operators.Abstractions;

public sealed class HandlerSignature: IEquatable<HandlerSignature>
{
    public required IPipeLoomEngine Engine { get; init; }
    
    public required PlOperatorArity Arity { get; init; }
    
    public required PlTypeDef ReturnType { get; init; }
    public required IReadOnlyList<PlTypeDef> ArgumentTypes { get; init; }
    
    public required bool IsVariadic { get; init; }
    
    private HandlerSignature() { }

    public override string ToString()
    {
        var arity = this.Arity.ToDisplayString();
        var arguments = string.Join(',', this.ArgumentTypes.Select(d => d.Name));
        
        return $"{arity}({arguments}): {this.ReturnType.Name}";
    }

    #region Factory methods
    
    public static HandlerSignature Nullary<TReturn>(IPipeLoomEngine engine)
    {
        return new HandlerSignature
        {
            Engine = engine,
            Arity = PlOperatorArity.Nullary,
            ReturnType = engine.TypeOf<TReturn>(),
            ArgumentTypes = [],
            IsVariadic = false
        };
    }
    
    public static HandlerSignature Unary<T1, TReturn>(IPipeLoomEngine engine)
    {
        return new HandlerSignature
        {
            Engine = engine,
            Arity = PlOperatorArity.Unary,
            ReturnType = engine.TypeOf<TReturn>(),
            ArgumentTypes = [engine.TypeOf<T1>()],
            IsVariadic = false
        };
    }
    
    public static HandlerSignature Binary<T1, T2, TReturn>(IPipeLoomEngine engine)
    {
        return new HandlerSignature
        {
            Engine = engine,
            Arity = PlOperatorArity.Binary,
            ReturnType = engine.TypeOf<TReturn>(),
            ArgumentTypes = [engine.TypeOf<T1>(), engine.TypeOf<T2>()],
            IsVariadic = false
        };
    }
    
    public static HandlerSignature Ternary<T1, T2, T3, TReturn>(IPipeLoomEngine engine)
    {
        return new HandlerSignature
        {
            Engine = engine,
            Arity = PlOperatorArity.Ternary,
            ReturnType = engine.TypeOf<TReturn>(),
            ArgumentTypes = [engine.TypeOf<T1>(), engine.TypeOf<T2>(), engine.TypeOf<T3>()],
            IsVariadic = false
        };
    }
    
    public static HandlerSignature Variadic<TVariadic, TReturn>(IPipeLoomEngine engine)
    {
        return new HandlerSignature
        {
            Engine = engine,
            Arity = PlOperatorArity.Variadic,
            ReturnType = engine.TypeOf<TReturn>(),
            ArgumentTypes = [engine.TypeOf<TVariadic>()],
            IsVariadic = true
        };
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HandlerSignature NonVariadic<T1, TReturn>(IPipeLoomEngine engine)
    {
        return Unary<T1, TReturn>(engine);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HandlerSignature NonVariadic<T1, T2, TReturn>(IPipeLoomEngine engine)
    {
        return Binary<T1, T2, TReturn>(engine);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HandlerSignature NonVariadic<T1, T2, T3, TReturn>(IPipeLoomEngine engine)
    {
        return Ternary<T1, T2, T3, TReturn>(engine);
    }
    
    #endregion

    #region Equality members
    
    public bool Equals(HandlerSignature? other)
    {
        if (other is null)
            return false;
        
        if (ReferenceEquals(this, other))
            return true;
        
        return this.Engine.Equals(other.Engine)
               && this.Arity == other.Arity
               && this.ReturnType.Equals(other.ReturnType)
               && this.IsVariadic == other.IsVariadic
               && this.ArgumentTypes.Count == other.ArgumentTypes.Count
               && this.ArgumentTypes.SequenceEqual(other.ArgumentTypes);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is HandlerSignature other && this.Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = HashCode.Combine(this.Engine, (int)this.Arity, this.ReturnType, this.IsVariadic);
        switch (this.Arity)
        {
            case PlOperatorArity.Nullary:
                break;
            case PlOperatorArity.Unary:
                hash = HashCode.Combine(hash, this.ArgumentTypes[0]);
                break;
            case PlOperatorArity.Binary:
                hash = HashCode.Combine(hash, this.ArgumentTypes[0], this.ArgumentTypes[1]);
                break;
            case PlOperatorArity.Ternary:
                hash = HashCode.Combine(hash, this.ArgumentTypes[0], this.ArgumentTypes[1], this.ArgumentTypes[2]);
                break;
            case PlOperatorArity.Variadic:
                hash = HashCode.Combine(hash, this.ArgumentTypes[0]);
                break;
            default:
                var argLength = this.ArgumentTypes.Count;
                for (var i = 0; i < argLength; i++)
                {
                    hash = HashCode.Combine(hash, this.ArgumentTypes[i]);
                }
                break;
        }

        return hash;
    }
    
    #endregion
}