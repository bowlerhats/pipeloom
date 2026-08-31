using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Operators.Abstractions;

public sealed class HandlerSignature: IEquatable<HandlerSignature>
{
    public required IPipeLoomEngine Engine { get; init; }
    
    public required PlOperatorArity Arity { get; init; }
    
    public required PlTypeDef ReturnType { get; init; }
    public required IReadOnlyList<PlTypeDef> ArgumentTypes { get; init; }
    
    public required bool IsVariadic { get; init; }

    public bool IsHomogenic => this.IsVariadic && this.ArgumentTypes.Count == 1;
    
    private HandlerSignature() {}

    public override string ToString()
    {
        var arity = this.Arity.ToDisplayString();
        var arguments = string.Join(',', this.ArgumentTypes.Select(d => d.Name));
        
        return $"{arity}({arguments}): {this.ReturnType.Name}";
    }

    public HandlerSignature AsVariadic()
    {
        if (this.IsVariadic)
            return this;
        
        return new HandlerSignature
        {
            Engine = this.Engine,
            Arity = PlOperatorArity.Variadic,
            ReturnType = this.ReturnType,
            ArgumentTypes = [this.Engine.CommonBaseOf(this.ArgumentTypes)],
            IsVariadic = true
        };
    }

    public bool IsStrictSuperSetOf(HandlerSignature other)
    {
        return !this.Equals(other) && this.IsSuperSetOf(other);
    }
    
    public bool IsSuperSetOf(HandlerSignature other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (this.Engine != other.Engine
            || this.Arity != other.Arity
            || this.IsVariadic != other.IsVariadic
            || this.ArgumentTypes.Count != other.ArgumentTypes.Count)
        {
            // has different shape
            return false;
        }

        if (other.ReturnType.Id != this.ReturnType.Id && !other.ReturnType.IsConvertibleTo(this.ReturnType))
            return false;

        for (var i = 0; i < this.ArgumentTypes.Count; i++)
        {
            var myArg = this.ArgumentTypes[i];
            var otherArg = other.ArgumentTypes[i];

            if (!ArgumentFits(myArg, myArg.ResolvesTo, otherArg, otherArg.ResolvesTo))
                return false;
        }

        return true;
    }

    private static bool ArgumentFits(PlTypeDef from, PlTypeDef fromResolved, PlTypeDef to, PlTypeDef toResolved)
    {
        return from.IsConvertibleTo(to)
               || from.IsConvertibleTo(toResolved)
               || fromResolved.IsConvertibleTo(to)
               || fromResolved.IsConvertibleTo(toResolved);
    }

    public bool IsStrictSubSetOf(HandlerSignature other)
    {
        return !this.Equals(other) && this.IsSubSetOf(other);
    }
    
    public bool IsSubSetOf(HandlerSignature other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return other.IsSuperSetOf(this);
    }
    

    #region Factory methods

    public static HandlerSignature Nullary(PlTypeDef returnType)
    {
        return new HandlerSignature
        {
            Engine = returnType.Engine,
            Arity = PlOperatorArity.Nullary,
            ReturnType = returnType,
            ArgumentTypes = [],
            IsVariadic = false
        };
    }
    
    public static HandlerSignature Nullary<TReturn>(IPipeLoomEngine engine)
    {
        return Nullary(engine.TypeOf<TReturn>());
    }

    public static HandlerSignature Unary(PlTypeDef returnType, PlTypeDef arg1)
    {
        return new HandlerSignature
        {
            Engine = returnType.Engine,
            Arity = PlOperatorArity.Unary,
            ReturnType = returnType,
            ArgumentTypes = [arg1],
            IsVariadic = false
        };
    }
    
    public static HandlerSignature Unary<T1, TReturn>(IPipeLoomEngine engine)
    {
        return Unary(engine.TypeOf<TReturn>(), engine.TypeOf<T1>());
    }
    
    public static HandlerSignature Binary(PlTypeDef returnType, PlTypeDef arg1, PlTypeDef arg2)
    {
        return new HandlerSignature
        {
            Engine = returnType.Engine,
            Arity = PlOperatorArity.Binary,
            ReturnType = returnType,
            ArgumentTypes = [arg1, arg2],
            IsVariadic = false
        };
    }
    
    public static HandlerSignature Binary<T1, T2, TReturn>(IPipeLoomEngine engine)
    {
        return Binary(engine.TypeOf<TReturn>(), engine.TypeOf<T1>(), engine.TypeOf<T2>());
    }
    
    public static HandlerSignature Ternary(PlTypeDef returnType, PlTypeDef arg1, PlTypeDef arg2, PlTypeDef arg3)
    {
        return new HandlerSignature
        {
            Engine = returnType.Engine,
            Arity = PlOperatorArity.Ternary,
            ReturnType = returnType,
            ArgumentTypes = [arg1, arg2, arg3],
            IsVariadic = false
        };
    }
    
    public static HandlerSignature Ternary<T1, T2, T3, TReturn>(IPipeLoomEngine engine)
    {
        return Ternary(engine.TypeOf<TReturn>(), engine.TypeOf<T1>(), engine.TypeOf<T2>(), engine.TypeOf<T3>());
    }

    public static HandlerSignature Variadic(PlTypeDef returnType, PlTypeDef vArg)
    {
        return new HandlerSignature
        {
            Engine = returnType.Engine,
            Arity = PlOperatorArity.Variadic,
            ReturnType = returnType,
            ArgumentTypes = [vArg],
            IsVariadic = true
        };
    }
    
    public static HandlerSignature Variadic(PlTypeDef returnType, PlTypeDef vImplicit, PlTypeDef vArg)
    {
        return new HandlerSignature
        {
            Engine = returnType.Engine,
            Arity = PlOperatorArity.Variadic,
            ReturnType = returnType,
            ArgumentTypes = [vImplicit, vArg],
            IsVariadic = true
        };
    }
    
    
    public static HandlerSignature Variadic<TVariadic, TReturn>(IPipeLoomEngine engine)
    {
        return Variadic(engine.TypeOf<TReturn>(), engine.TypeOf<TVariadic>());
    }
    
    public static HandlerSignature Variadic<TImplicit, TVariadic, TReturn>(IPipeLoomEngine engine)
    {
        return Variadic(engine.TypeOf<TReturn>(), engine.TypeOf<TImplicit>(), engine.TypeOf<TVariadic>());
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

    public static HandlerSignature From(PlTypeDef returnType, IEnumerable<PlTypeDef> args)
    {
        var engine = returnType.Engine;

        var argList = args.ToList();
        
        var arity = engine.GuessArity(argList) ?? throw new PipeLoomException("Couldn't guess arity?!");
        var isVariadic = arity == PlOperatorArity.Variadic;

        return new HandlerSignature
        {
            Engine = engine,
            Arity = arity,
            ReturnType = returnType,
            ArgumentTypes = isVariadic ? [engine.CommonBaseOf(argList)] : argList,
            IsVariadic = isVariadic
        };
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