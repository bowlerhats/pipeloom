using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Pools;

namespace PipeLoom.Engine;

public interface IWeaveContext
{
    IPipeLoomEngine Engine { get; }
    WeavePlan Plan { get; }
    
    IPoolSet Pools { get; }
}

internal sealed class WeaveContext : IWeaveContext, IDisposable
{
    public PipeLoomEngine Engine { get; }
    public WeavePlan Plan { get; }

    public PoolSet Pools { get; }

    IPipeLoomEngine IWeaveContext.Engine => this.Engine;
    IPoolSet IWeaveContext.Pools => this.Pools;

    private IObjectPool<StepState> _statePool;
    private ArrayPool<Variant> _variantPool;
    private IObjectPool<IBundle<Variant>> _variantBundlePool;
    
    // non-null when poolset was leased, null when it's local/static
    private Lease<PoolSet>? _poolsetLease;

    public WeaveContext(PipeLoomEngine engine, WeavePlan plan)
    {
        this.Engine = engine;
        this.Plan = plan;
        
        this.Pools = this.ChoosePoolSet();
        _statePool = this.Pools.GetObjectPool<StepState>(_ => new StepState(), MagicNumbers.StepStatePoolSize);
        _variantPool = this.Pools.GetArrayPool<Variant>();
        _variantBundlePool = this.Pools.GetObjectPool<IBundle<Variant>>(
            _ => throw new NotImplementedException(),
            MagicNumbers.VariantBundlePoolsize);
    }

    public void Dispose()
    {
        if (_poolsetLease.HasValue)
        {
            this.Pools.ReleaseAllTouched();
            
            _poolsetLease.Value.Dispose();
        }
        else
        {
            this.Pools.Dispose();
        }
    }

    public IBundle<T> NewBundle<T>()
    {
        throw new NotImplementedException();
    }

    public ValueTask<Variant> Step()
    {
        return this.Step(this.Plan.RootNode, null, null);
    }

    private ValueTask<Variant> StepAnalyzed(WeaveNode node, StepState parentState)
    {
        var carry = parentState.Carry;
        if (carry.IsUndefined)
        {
            // Step as-is, nothing special
            return this.Step(node, parentState, null);
        }
        
        if (node.Handler is null)
            throw new PipeLoomException($"Missing operator handler for '{node.OperatorName}'");

        switch (node.Handler.Role)
        {
            case HandlerRole.None:
                // Step as-is, ignore carry
                return this.Step(node, parentState, null);
            
            // bundle -> bundle
            case HandlerRole.Bundler:
                if (carry.Tag is not PlBundle && this.Engine.TryConvert(in carry, this.Engine.WellKnown.Bundle, out var converted))
                {
                    return this.Step(node, parentState, converted);
                }
                
                break;
            
            case HandlerRole.Reducer:
                if (carry.Tag is not PlMany)
                {
                    if (carry.TryUnpack<IReadOnlyBundle>(out var bundle, reinterpret: true))
                    {
                        return this.ReduceBundle(node, parentState, bundle);
                    }

                    if (this.Engine.TryConvert(in carry, this.Engine.WellKnown.Bundle, out var transformableBundle))
                    {
                        return this.ReduceBundle(node, parentState, transformableBundle.Unpack<IReadOnlyBundle>());
                    }
                    
                    if (this.Engine.TryConvert(in carry, this.Engine.WellKnown.ManyOfVariant, out var transformableMany))
                    {
                        return this.Step(node, parentState, transformableMany);
                    }
                    
                    return this.Step(node, parentState, null);
                }
                
                break;
            case HandlerRole.Transformer:
                if (carry.Tag is not PlMany)
                {
                    if (carry.TryUnpack<IReadOnlyBundle>(out var bundle, reinterpret: true))
                    {
                        return this.TransformBundle(node, parentState, bundle);
                    }

                    if (this.Engine.TryConvert(in carry, this.Engine.WellKnown.Bundle, out var transformableBundle))
                    {
                        return this.TransformBundle(node, parentState, transformableBundle.Unpack<IReadOnlyBundle>());
                    }
                    
                    if (this.Engine.TryConvert(in carry, this.Engine.WellKnown.ManyOfVariant, out var transformableMany))
                    {
                        return this.Step(node, parentState, transformableMany);
                    }
                    
                    return this.Step(node, parentState, null);
                }
                
                break;
            
            // one -> one
            case HandlerRole.Mapper:
                if (carry.TryUnpack<IReadOnlyBundle>(out var mapingBundle, reinterpret: true))
                {
                    return this.MapBundle(node, parentState, mapingBundle);
                }

                if (carry.TryUnpack<Many<Variant>>(out var manyForMapping))
                {
                    return this.MapMany(node, parentState, manyForMapping);
                }

                if (this.Engine.TryConvert(in carry, this.Engine.WellKnown.ManyOfVariant, out var mapConverted))
                {
                    return this.MapMany(node, parentState, mapConverted.Unpack<Many<Variant>>());
                }
                
                break;
            case HandlerRole.Expander:
                
                if (carry.TryUnpack<IReadOnlyBundle>(out var expandingBundle, reinterpret: true))
                {
                    return this.ExpandBundle(node, parentState, expandingBundle);
                }
                
                break;
        }
        
        // No special handling, call as-is with implicitly forwarded carry
        return this.Step(node, parentState, carry);
    }

    private async ValueTask<Variant> Step(WeaveNode node, StepState? parentState, Variant? @implicit)
    {
        var handler = node.Handler;
        
        if (handler is null)
            throw new PipeLoomException($"Missing operator handler for '{node.OperatorName}'");
        
        var childrenCoount = node.Children.Count;
        
        using var stateLease = _statePool.Lease();
        var state = stateLease.Item;

        var argBuffer = _variantPool.Rent(childrenCoount);
        try
        {
            state.Bind(this, node, parentState);
            
            var argPos = 0;
            
            if (@implicit.HasValue)
            {
                argBuffer[argPos++] = @implicit.Value;
            }
            
            for (var i = 0; i < childrenCoount; i++)
            {
                var child = node.Children[i];
                if (!child.IsEnabled || child.IsFuseOnly)
                    continue;

                if (!child.IsArgument)
                {
                    await this.StepAnalyzed(child, state);
                }
                else
                {
                    Variant childOutput;
                        
                    var argType = handler.Signature.ArgumentTypes[argPos];
                    
                    if (argType is IPlCustomInputArgProvider argProvider
                        && argProvider.TryProvide(state, i, out var provided))
                    {
                        childOutput = provided;
                    }
                    else
                    {
                        childOutput = await this.StepAnalyzed(child, state);
                    }
                    
                    argBuffer[argPos++] = childOutput;
                }
            }

            ReadOnlyMemory<Variant> arguments = argPos > 0 ? argBuffer.AsMemory(0, argPos) : Memory<Variant>.Empty;
            return await handler.Adapter.Call(state, in arguments);
        }
        finally
        {
            _variantPool.Return(argBuffer, true);
            
            state.Unbind();
        }
    }
    
    private async ValueTask<Variant> TransformBundle(WeaveNode node, StepState parentState, IReadOnlyBundle bundle)
    {
        Debug.Assert(node.Handler?.Role == HandlerRole.Transformer);

        var res = this.NewBundle<Variant>();
        
        var partitionCount = bundle.Partitions.Count;
        for (var i = 0; i < partitionCount; i++)
        {
            var partition = bundle.Partitions[i];

            var leaf = Variant.From(partition.Leaf);

            var transformed = await this.Step(node, parentState, leaf);

            if (!transformed.TryUnpack<Many<Variant>>(out var transformedLeaf)
                && !this.Engine.TryConvert(in transformed, out transformedLeaf))
            {
                throw new PipeLoomException("Transformer expected to return Many<>");
            }
            
            res.SetMany(partition, transformedLeaf);
        }

        return res.ToVariant();
    }
    
    private async ValueTask<Variant> ReduceBundle(WeaveNode node, StepState parentState, IReadOnlyBundle bundle)
    {
        Debug.Assert(node.Handler?.Role == HandlerRole.Reducer);
        
        var res = this.NewBundle<Variant>();
        
        var partitionCount = bundle.Partitions.Count;
        for (var i = 0; i < partitionCount; i++)
        {
            var partition = bundle.Partitions[i];

            var leaf = Variant.From(partition.Leaf);

            var reduced = await this.Step(node, parentState, leaf);
            
            res.SetSingle(partition, reduced);
        }

        return res.ToVariant();
    }

    private async ValueTask<Variant> MapBundle(WeaveNode node, StepState parentState, IReadOnlyBundle bundle)
    {
        Debug.Assert(node.Handler?.Role == HandlerRole.Mapper);

        var res = this.NewBundle<Variant>();
        
        var partitionCount = bundle.Partitions.Count;
        for (var i = 0; i < partitionCount; i++)
        {
            var partition = bundle.Partitions[i];

            var mapped = await this.MapMany(node, parentState, partition.Leaf);
            
            res.SetMany(partition, mapped.Unpack<Many<Variant>>());
        }

        return res.ToVariant();
    }
    
    private async ValueTask<Variant> MapMany(WeaveNode node, StepState parentState, Many<Variant> many)
    {
        Debug.Assert(node.Handler?.Role == HandlerRole.Mapper);

        var itemCount = many.Length;
        
        var mapped = new List<Variant>(itemCount);
        for (var i = 0; i < itemCount; i++)
        {
            mapped[i] = await this.Step(node, parentState, many[i]);
        }

        return Many<Variant>.Wrap(mapped).ToVariant();
    }

    private ValueTask<Variant> ExpandBundle(WeaveNode node, StepState parentState, IReadOnlyBundle bundle)
    {
        Debug.Assert(node.Handler?.Role == HandlerRole.Expander);
        throw new NotImplementedException();
    }
    
    private PoolSet ChoosePoolSet()
    {
        _poolsetLease = this.Engine.PoolSets.TryLease()?.As<PoolSet>();
        if (_poolsetLease.HasValue)
        {
            return _poolsetLease;
        }
        
        return new StaticPoolSet();
    }
}