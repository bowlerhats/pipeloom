using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Operators.Abstractions;


public abstract class PlOperatorClass
{
    public string Name { get; }
    
    // public virtual List<string> Aliases { get; } = [];

    public virtual bool IsClosed => false;
    public virtual bool IsFuseOnly => false;
    public virtual bool IsVoid => false;

    //IReadOnlyCollection<string> IPlOperatorClass.Aliases => this.Aliases;
    
    public IPipeLoomEngine Engine { get; }

    public IReadOnlyList<OperatorHandler> Handlers => _handlers;
    
    private List<OperatorHandler> _handlers = [];
    
    protected PlOperatorClass(IPipeLoomEngine engine, string name)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        this.Engine = engine;
        this.Name = name;
    }

    public virtual void RegisterHandlers(PlOperatorRegistrator registrator)
    {
    }
    
    internal void AddHandler(OperatorHandler opHandler)
    {
        ArgumentNullException.ThrowIfNull(opHandler);
        if (opHandler.OperatorClass != this)
        {
            throw new ArgumentException(
                $"Attempted to add handler of class '{opHandler.OperatorClass.Name}' to op class '{this.Name}'",
                nameof(opHandler));
        }

        if (_handlers.Any(d => d.Signature.Equals(opHandler.Signature)))
        {
            throw new ArgumentOutOfRangeException(
                $"Operator class '{this.Name}' already contains a handler with same signature '{opHandler.Signature}'");
        }
        
        _handlers.Add(opHandler);
    }

    internal OperatorHandler? FindMostSpecific(HandlerSignature searched, bool onlyWithRole = false)
    {
        ArgumentNullException.ThrowIfNull(searched);

        OperatorHandler? res = null;
        var score = -1;
        
        // choosing last fit of highest scores
        
        foreach (var handler in _handlers)
        {
            if (onlyWithRole && handler.Role == HandlerRole.None)
                continue;
            
            var fitScore = handler.FitScore(searched);
            if (fitScore >= 0 && fitScore >= score)
            {
                res = handler;
                score = fitScore;
            }
        }

        return res;
    }

    public virtual ValueTask<PreFuseFlags> PreFuse(WeaveNode node)
    {
        return ValueTask.FromResult(PreFuseFlags.None);
    }

    public virtual ValueTask<PostFuseFlags> PostFuse(WeaveNode node)
    {
        return ValueTask.FromResult(PostFuseFlags.None);
    }

    public virtual OperatorHandler? ChooseHandler(WeaveNode node)
    {
        OperatorHandler? candidate = null;

        var argCount = node.CountArguments();

        var returnType = node.RequiredReturnType ?? this.Engine.WellKnown.Variant;

        var firstArg = node.Arguments.ElementAtOrDefault(0)?.ReturnType!;
        var secondArg = node.Arguments.ElementAtOrDefault(1)?.ReturnType!;
        var thirdArg = node.Arguments.ElementAtOrDefault(2)?.ReturnType!;
        
        if (node.CarryType is not null)
        {
            switch (argCount)
            {
                case 0:
                    candidate = this.FindMostSpecific(HandlerSignature.Unary(returnType, node.CarryType), true);
                    break;
                case 1:
                    candidate = this.FindMostSpecific(HandlerSignature.Binary(returnType, node.CarryType, firstArg), true);
                    break;
                case 2:
                    candidate = this.FindMostSpecific(HandlerSignature.Ternary(returnType, node.CarryType, firstArg, secondArg), true);
                    break;
            }
        }

        if (candidate is not null)
            return candidate;
        
        switch (argCount)
        {
            case 0:
                candidate = this.FindMostSpecific(HandlerSignature.Nullary(returnType));
                break;
            case 1:
                candidate = this.FindMostSpecific(HandlerSignature.Unary(returnType, firstArg));
                break;
            case 2:
                candidate = this.FindMostSpecific(HandlerSignature.Binary(returnType, firstArg, secondArg));
                break;
            case 3:
                candidate = this.FindMostSpecific(HandlerSignature.Ternary(returnType, firstArg, secondArg, thirdArg));
                break;
        }

        if (node.CarryType is not null)
        {
            candidate ??= this.FindMostSpecific(HandlerSignature.Variadic(returnType, node.CarryType, firstArg), true);
        }
        
        candidate ??= this.FindMostSpecific(HandlerSignature.Variadic(returnType, firstArg));

        return candidate;
    }
}
