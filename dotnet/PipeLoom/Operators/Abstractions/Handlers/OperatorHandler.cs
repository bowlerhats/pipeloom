using System;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Adapters;

namespace PipeLoom.Operators.Abstractions.Handlers;

public abstract class OperatorHandler
{
    public const int FitScoreExact = 1000;
    public const int FitScoreNever = -1;
    
    public IPipeLoomEngine Engine { get; }
    
    public PlOperatorClass OperatorClass { get; }
    
    public PlOperatorArity Arity { get; }
    
    public HandlerSignature Signature { get; protected set; }

    public HandlerRole Role { get; set; } = HandlerRole.None;
    
    public MethodAdapter Adapter { get; }

    public bool HasImplicit
        => this.Role != HandlerRole.None || this.Signature is { IsVariadic: true, IsHomogenic: false };
    
    protected Func<WeaveNode, PlTypeDef, PlTypeDef>? Narrower { get; set; }
    
    protected OperatorHandler(
        PlOperatorClass operatorClass,
        PlOperatorArity arity,
        MethodAdapter adapter
        )
    {
        this.OperatorClass = operatorClass;
        this.Engine = operatorClass.Engine;
        this.Arity = arity;
        this.Adapter = adapter;

        this.Signature = arity switch
        {
            PlOperatorArity.Nullary => HandlerSignature.Nullary<Variant>(this.Engine),
            PlOperatorArity.Unary => HandlerSignature.Unary<Variant, Variant>(this.Engine),
            PlOperatorArity.Binary => HandlerSignature.Binary<Variant, Variant, Variant>(this.Engine),
            PlOperatorArity.Ternary => HandlerSignature.Ternary<Variant, Variant, Variant, Variant>(this.Engine),
            PlOperatorArity.Variadic => HandlerSignature.Variadic<Variant, Variant>(this.Engine),
            _ => throw new ArgumentOutOfRangeException(nameof(arity), arity, null)
        };
    }

    public virtual int FitScore(HandlerSignature expected)
    {
        if (this.Signature.Equals(expected))
            return FitScoreExact;

        var argCount = expected.ArgumentTypes.Count;

        if (this.Arity != expected.Arity || this.Signature.ArgumentTypes.Count != argCount)
        {
            return FitScoreNever;
        }

        // todo: implement converter scoring
        
        const int argMatchScore = 10;
        const int argConvertibleScore = 5;
        
        var score = 100;

        // if expectation is Variant return it is equivalent to "can return anything"
        // otherwise we have to match and score the return
        if (expected.ReturnType != this.Engine.WellKnown.Variant)
        {
            if (this.Signature.ReturnType == expected.ReturnType)
            {
                score += argMatchScore;
            } else if (this.Signature.ReturnType.IsConvertibleTo(expected.ReturnType))
            {
                score += argConvertibleScore;
            }
            else
            {
                // handler cannot match expected return type
                return FitScoreNever;
            }
        }
        
        for (var i = 0; i < argCount; i++)
        {
            var thisArg = this.Signature.ArgumentTypes[i];
            var thisResolvedArg = thisArg.ResolvesTo;
            var otherArg = expected.ArgumentTypes[i];
            var otherResolvedArg = otherArg.ResolvesTo;
            
            if (thisArg.Equals(otherArg)
                || thisArg.Equals(otherResolvedArg)
                || thisResolvedArg.Equals(otherArg)
                || thisResolvedArg.Equals(otherResolvedArg))
            {
                // favor more matches at start of arguments
                var bias = argCount - 1;
                score += argMatchScore + bias;
            } else if (otherArg.IsConvertibleTo(thisArg)
                       || otherArg.IsConvertibleTo(thisResolvedArg)
                       || otherResolvedArg.IsConvertibleTo(thisArg)
                       || otherResolvedArg.IsConvertibleTo(thisResolvedArg))
            {
                score += argConvertibleScore;
            }
            else
            {
                return FitScoreNever;
            }
        }

        return score;
    }

    //public abstract ValueTask<Variant> Call(IStepState state, scoped in ReadOnlyMemory<Variant> arguments);

    public PlTypeDef NarrowReturnType(WeaveNode node)
    {
        var @implicit = this.ImplicitNarrow(node);

        return this.Narrower?.Invoke(node, @implicit) ?? @implicit;
    }

    public PlTypeDef ImplicitNarrow(WeaveNode node)
    {
        if (node.NarrowedReturnType is not null)
        {
            return node.NarrowedReturnType.IsConvertibleTo(this.Signature.ReturnType)
                ? node.NarrowedReturnType
                : this.Signature.ReturnType;
        }
        
        return this.Signature.ReturnType;
    }

    public override string ToString()
    {
        return this.Signature != null!
            ? $"{this.OperatorClass.Name} {this.Signature}"
            : $"{this.OperatorClass.Name} (???): ???";
    }
}

public abstract class OperatorHandler<TSelf> : OperatorHandler
    where TSelf: OperatorHandler<TSelf>
{
    public TSelf Self => (TSelf)this;
    
    protected OperatorHandler(PlOperatorClass operatorClass, PlOperatorArity arity, MethodAdapter adapter)
        : base(operatorClass, arity, adapter)
    {
    }
    
    public TSelf WithRole(HandlerRole role)
    {
        this.Role = role;
        return this.Self;
    }

    public TSelf ReturnAs(Func<WeaveNode, PlTypeDef, PlTypeDef> narrower)
    {
        this.Narrower = narrower;
        return this.Self;
    }
}