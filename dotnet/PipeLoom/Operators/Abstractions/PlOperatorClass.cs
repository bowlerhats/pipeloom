using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Operators.Abstractions;


public abstract class PlOperatorClass //: IPlOperatorClass
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

    // internal IEnumerable<OperatorHandler> LookupMatchingHandlers(HandlerSignature signature)
    // {
    //     foreach (var handler in _handlers)
    //     {
    //         if (signature.Arity != handler.Arity)
    //             continue;
    //
    //         var hSignature = handler.Signature;
    //         
    //         if (!hSignature.ReturnType.IsAssignableTo(signature.ReturnType))
    //             continue;
    //         
    //         Debug.Assert(signature.ArgumentTypes.Count == hSignature.ArgumentTypes.Count);
    //
    //         var matching = true;
    //         for (var i = 0; i < hSignature.ArgumentTypes.Count; i++)
    //         {
    //             matching &= signature.ArgumentTypes[i].IsAssignableTo(hSignature.ArgumentTypes[i]);
    //         }
    //
    //         if (matching)
    //             yield return handler;
    //     }
    // }

    internal OperatorHandler? FindMostSpecific(HandlerSignature searched, bool onlyWithRole = false)
    {
        ArgumentNullException.ThrowIfNull(searched);

        OperatorHandler? res = null;
        
        // Search for a direct one
        foreach (var handler in _handlers)
        {
            if (!searched.IsSuperSetOf(handler.Signature))
                continue; // handler is out of bounds of search signature
            
            if (onlyWithRole && handler.Role == HandlerRole.None)
                continue;

            res ??= handler;
            
            if (handler.Signature.IsStrictSubSetOf(res.Signature))
            {
                // found a narrower match 
                res = handler;
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
        if (node.CarryType is not null)
        {
            var carriedLimit = HandlerSignature.From(
                this.Engine.WellKnown.Variant,
                node.Arguments.Select(d => d.ReturnType).Prepend(node.CarryType));

            var candidate = this.FindMostSpecific(carriedLimit, true);
            if (candidate is not null)
                return candidate;
        }

        var searchLimit = HandlerSignature.From(this.Engine.WellKnown.Variant, node.Arguments.Select(d => d.ReturnType));
        
        return this.FindMostSpecific(searchLimit);
    }
}
