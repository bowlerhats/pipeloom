using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Bundles;
using PipeLoom.Engine.Pools;

namespace PipeLoom.Engine;

public interface IWeaveContext
{
    IPipeLoomEngine Engine { get; }
    WeavePlan Plan { get; }
    
    IPoolSet Pools { get; }
    
    IBundleFactory Bundles { get; }

    ValueTask<T> StepDetached<T, TCarry>(Detached<T> detached, TCarry carry);
    ValueTask<T> StepDetached<T, TCarry>(Detached<T> detached, TCarry carry, IStepState state);
}

internal sealed class WeaveContext : IWeaveContext, IDisposable
{
    public PipeLoomEngine Engine { get; }
    public WeavePlan Plan { get; }

    public PoolSet Pools { get; }

    public IBundleFactory Bundles => _bundleFactory;

    IPipeLoomEngine IWeaveContext.Engine => this.Engine;
    IPoolSet IWeaveContext.Pools => this.Pools;

    private IObjectPool<StepState> _statePool;
    private ArrayPool<Variant> _variantArrayPool;
    
    // non-null when poolset was leased, null when it's local/static
    private Lease<MemCachedPoolSet>? _poolsetLease;
    private BundleFactory _bundleFactory;

    private bool _disposed;

    public WeaveContext(PipeLoomEngine engine)
        : this(engine, new WeavePlan(engine))
    {
    }
    
    public WeaveContext(PipeLoomEngine engine, WeavePlan plan)
    {
        this.Plan = plan;
        this.Engine = engine;
        
        this.Pools = this.ChoosePoolSet();
        
        _statePool = this.Pools.StepStates;
        _variantArrayPool = this.Pools.GetArrayPool<Variant>();

        _bundleFactory = this.Pools.BundleFactories.Rent();
        _bundleFactory.Bind(this);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true))
           return;
        
        this.Pools.BundleFactories.Return(_bundleFactory);
        
        if (_poolsetLease.HasValue)
        {
            this.Pools.ReleaseAllTouched();
            
            _poolsetLease.Value.Dispose();
            
            _poolsetLease = null;
        }
        else
        {
            this.Pools.Dispose();
        }
    }

    public async ValueTask<T> StepDetached<T, TCarry>(Detached<T> detached, TCarry carry)
    {
        using var stateLease = _statePool.LeaseUntracked();
        var state = stateLease.Item;
        try
        {
            state.Bind(this, (WeaveNode)detached.Node, null);

            return await this.StepDetached(detached, carry, state);
        }
        finally
        {
            state.Unbind();
        }
    }
    
    public async ValueTask<T> StepDetached<T, TCarry>(Detached<T> detached, TCarry carry, IStepState state)
    {
        var vCarry = typeof(TCarry) == typeof(Variant)
            ? Variant.VerbatimCopyUnsafe(carry)
            : Variant.From(carry, this.Engine);
        
        var stepResult = vCarry.IsUndefined
            ? await this.StepAnalyzed((WeaveNode)detached.Node, (StepState)state)
            : await this.StepAnalyzed((WeaveNode)detached.Node, (StepState)state, vCarry);

        return stepResult.Unpack<T>();
    }
 
    public ValueTask<Variant> Step(Variant? carry)
    {
        return carry.HasValue
            ? this.Step(this.Plan.RootNode, null, true, carry)
            : this.Step(this.Plan.RootNode, null, false, null);
    }

    private ValueTask<Variant> StepAnalyzed(WeaveNode node, StepState parentState, Variant? newCarry = null)
    {
        var carry = newCarry ?? parentState.Carry;
        
        if (node.Handler is null)
            throw new PipeLoomException($"Missing operator handler for '{node.OperatorName}'");

        // switch (node.Handler.Role)
        // {
        //     // case HandlerRole.None:
        //     //     // Step as-is, ignore carry
        //     //     return this.Step(node, parentState, null);
        //     
        //     // // bundle -> bundle
        //     // case HandlerRole.Bundler:
        //     //     if (carry.Tag is not PlReadOnlyBundleOf
        //     //         && this.Engine.Conversions.TryConvert(this, in carry, this.Engine.WellKnown.ReadOnlyBundleOfVariant, out var converted))
        //     //     {
        //     //         return this.Step(node, parentState, converted);
        //     //     }
        //     //     
        //     //     break;
        //     //
        //     // case HandlerRole.Reducer:
        //     //     if (carry.Tag is not PlManyOf)
        //     //     {
        //     //         // if (carry.TryUnpack<IReadOnlyBundle>(out var bundle, reinterpret: true))
        //     //         // {
        //     //         //     return this.ReduceBundle(node, parentState, bundle);
        //     //         // }
        //     //
        //     //         if (this.Engine.Conversions.TryConvert(this, in carry, this.Engine.WellKnown.ReadOnlyBundleOfVariant, out var transformableBundle))
        //     //         {
        //     //             return this.ReduceBundle(node, parentState, transformableBundle.Unpack<IReadOnlyBundle<Variant>>());
        //     //         }
        //     //         
        //     //         if (this.Engine.Conversions.TryConvert(this, in carry, this.Engine.WellKnown.ManyOfVariant, out var transformableMany))
        //     //         {
        //     //             return this.Step(node, parentState, transformableMany);
        //     //         }
        //     //         
        //     //         return this.Step(node, parentState, null);
        //     //     }
        //     //     
        //     //     break;
        //     // case HandlerRole.Transformer:
        //     //     if (carry.Tag is not PlMany)
        //     //     {
        //     //         // if (carry.TryUnpack<IReadOnlyBundle>(out var bundle, reinterpret: true))
        //     //         // {
        //     //         //     return this.TransformBundle(node, parentState, bundle);
        //     //         // }
        //     //
        //     //         if (this.Engine.Conversions.TryConvert(this, in carry, this.Engine.WellKnown.ReadOnlyBundleOfVariant, out var transformableBundle))
        //     //         {
        //     //             return this.TransformBundle(node, parentState, transformableBundle.Unpack<IReadOnlyBundle<Variant>>());
        //     //         }
        //     //         
        //     //         if (this.Engine.Conversions.TryConvert(this, in carry, this.Engine.WellKnown.ManyOfVariant, out var transformableMany))
        //     //         {
        //     //             return this.Step(node, parentState, transformableMany);
        //     //         }
        //     //         
        //     //         return this.Step(node, parentState, null);
        //     //     }
        //     //     
        //     //     break;
        //     //
        //     // // one -> one
        //     // case HandlerRole.Mapper:
        //     //     // if (carry.TryUnpack<IReadOnlyBundle>(out var mapingBundle, reinterpret: true))
        //     //     // {
        //     //     //     return this.MapBundle(node, parentState, mapingBundle);
        //     //     // }
        //     //
        //     //     if (carry.TryUnpack<Many<Variant>>(out var manyForMapping))
        //     //     {
        //     //         return this.MapMany(node, parentState, manyForMapping);
        //     //     }
        //     //
        //     //     if (this.Engine.Conversions.TryConvert(this, in carry, this.Engine.WellKnown.ManyOfVariant, out var mapConverted))
        //     //     {
        //     //         return this.MapMany(node, parentState, mapConverted.Unpack<Many<Variant>>());
        //     //     }
        //     //     
        //     //     break;
        //     // case HandlerRole.Expander:
        //     //     
        //     //     if (carry.TryUnpack<IReadOnlyBundle>(out var expandingBundle, reinterpret: true))
        //     //     {
        //     //         return this.ExpandBundle(node, parentState, expandingBundle);
        //     //     }
        //     //     
        //     //     break;
        // }
        
        if (!node.Handler.HasImplicit)
            return this.Step(node, parentState, false, carry);

        var fitCarry = node.Handler.Signature.IsVariadic
                       || node.CountArguments() + 1 == node.Handler.Signature.ArgumentTypes.Count;

        return this.Step(node, parentState, fitCarry, carry);
    }

    private async ValueTask<Variant> Step(WeaveNode node, StepState? parentState, bool useCarry, Variant? carry)
    {
        var handler = node.Handler;
        
        if (handler is null)
            throw new PipeLoomException($"Missing operator handler for '{node.OperatorName}'");
        
        var childrenCount = node.Children.Count;
        
        using var stateLease = _statePool.LeaseUntracked();
        var state = stateLease.Item;

        var argBuffer = _variantArrayPool.Rent(childrenCount + 1);
        try
        {
            state.Bind(this, node, parentState);

            if (carry.HasValue)
            {
                state.Carry = carry.Value;
            }
            
            var argPos = 0;
            
            if (useCarry)
            {
                argBuffer[argPos] = this.Engine.Conversions.Convert(this, state.Carry, handler.Signature.ArgumentTypes[argPos]);
                argPos++;
            }
            
            for (var i = 0; i < childrenCount; i++)
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
                    if (!handler.Signature.IsVariadic && argPos >= handler.Signature.ArgumentTypes.Count)
                        throw new PipeLoomException($"Too much arguments trying to call '{handler}'");

                    Variant childOutput;
                    
                    var argType = handler.Signature.IsVariadic
                        ? handler.Signature.ArgumentTypes[handler.HasImplicit ? 1 : 0]
                        : handler.Signature.ArgumentTypes[argPos];
                    
                    if (argType is IPlCustomInputArgProvider argProvider
                        && argProvider.TryProvide(state, i, out var provided))
                    {
                        childOutput = provided;
                    }
                    else
                    {
                        childOutput = await this.StepAnalyzed(child, state);
                    }
                    
                    argBuffer[argPos++] = this.Engine.Conversions.Convert(this, in childOutput, argType);
                }
            }
            
            if (!handler.Signature.IsVariadic && argPos < handler.Signature.ArgumentTypes.Count)
                throw new PipeLoomException($"Expected {handler.Signature.ArgumentTypes.Count} arguments but got {argPos} to call '{handler}'");

            ReadOnlyMemory<Variant> arguments = argPos > 0 ? argBuffer.AsMemory(0, argPos) : Memory<Variant>.Empty;
            var result = await handler.Adapter.Call(state, in arguments);
            
            return this.Engine.Conversions.Convert(this, result, handler.Signature.ReturnType);
        }
        finally
        {
            _variantArrayPool.Return(argBuffer, true);
            
            state.Unbind();
        }
    }

    // private async ValueTask<Variant> TransformBundle(WeaveNode node, StepState parentState, IReadOnlyBundle<Variant> bundle)
    // {
    //     Debug.Assert(node.Handler?.Role == HandlerRole.Transformer);
    //
    //     var res = this.Bundles.Create<Variant>();
    //
    //     var paths = bundle.Paths;
    //     for (var i = 0; i < paths.Length; i++)
    //     {
    //         var path = paths.Span[i];
    //         var leaf = bundle.GetLeafAsPackedToVariant(path);
    //
    //         var transformed = await this.Step(node, parentState, leaf);
    //         
    //         if (transformed.TryUnpack<Many<Variant>>(out var converted)
    //             || this.Engine.Conversions.TryConvert(this, in transformed, out converted))
    //         {
    //             res.SetLeaf(path, converted);
    //         }
    //         else
    //         {
    //             throw new PipeLoomException("Transformer expected to return Many<> or any convertible to Many<>");
    //         }
    //     }
    //     
    //     return res.PackAsVariant();
    // }
    
    // private async ValueTask<Variant> TransformBundle(WeaveNode node, StepState parentState, IReadOnlyBundle bundle)
    // {
    //     Debug.Assert(node.Handler?.Role == HandlerRole.Transformer);
    //     throw new NotImplementedException();
    //
    //     // var needsInputconversion = node.CarryType != bundle.LeafType;
    //     //
    //     // var res = this.Bundles.Create<Variant>();
    //     //
    //     // var paths = bundle.Paths;
    //     // for (var i = 0; i < paths.Length; i++)
    //     // {
    //     //     var path = paths.Span[i];
    //     //     var leaf = bundle.GetLeafAsPackedToVariant(path);
    //     //     
    //     //     if (needsInputconversion && !this.Engine.Conversions.TryConvert(this, leaf, out ))
    //     //     
    //     //     var transformed = await this.Step(node, parentState, leaf);
    //     //
    //     //     if (transformed.Tag != bundle.LeafType)
    //     //     {
    //     //         if (this.Engine.Conversions.TryConvert(this, in transformed, bundle.LeafType , out var cTransformed))
    //     //         {
    //     //             res.SetLeaf(path, cTransformed);
    //     //         }
    //     //         else
    //     //         {
    //     //             throw new PipeLoomException("Transformer expected to return Many<>");
    //     //         }
    //     //     }
    //     //     else
    //     //     {
    //     //         res.SetLeaf(path, transformed);
    //     //     }
    //     // }
    //     //
    //     // return res.PackAsVariant();
    // }
    
    // private async ValueTask<Variant> ReduceBundle(WeaveNode node, StepState parentState, IReadOnlyBundle<Variant> bundle)
    // {
    //     Debug.Assert(node.Handler?.Role == HandlerRole.Reducer);
    //     
    //     var res = this.Bundles.Create<Variant>();
    //
    //     var paths = bundle.Paths;
    //     for (var i = 0; i < paths.Length; i++)
    //     {
    //         var path = paths.Span[i];
    //         var leaf = bundle.GetLeafAsPackedToVariant(path);
    //         
    //         var reduced = await this.Step(node, parentState, leaf);
    //         
    //         res.SetLeaf(path, reduced);
    //     }
    //
    //     return res.PackAsVariant();
    // }

    // private async ValueTask<Variant> MapBundle(WeaveNode node, StepState parentState, IReadOnlyBundle<Variant> bundle)
    // {
    //     Debug.Assert(node.Handler?.Role == HandlerRole.Mapper);
    //
    //     var res = this.Bundles.Create<Variant>();
    //     
    //     // var paths = bundle.Paths;
    //     // var pathCount = paths.Count;
    //     // for (var i = 0; i < pathCount; i++)
    //     // {
    //     //     var path = bundle.Paths[i];
    //     //     
    //     //     var mapped = await this.MapMany(node, parentState, bundle.Erased.GetErasedLeaf(path));
    //     //     
    //     //     res.SetLeaf(path, mapped.Unpack<Many<Variant>>());
    //     // }
    //
    //     return res.PackAsVariant();
    // }
    //
    // private async ValueTask<Variant> MapMany(WeaveNode node, StepState parentState, Many<Variant> many)
    // {
    //     Debug.Assert(node.Handler?.Role == HandlerRole.Mapper);
    //
    //     var itemCount = many.Length;
    //
    //     var mapped = _variantArrayPool.Rent(itemCount);
    //     try
    //     {
    //         // todo: parallelize
    //         for (var i = 0; i < itemCount; i++)
    //         {
    //             mapped[i] = await this.Step(node, parentState, many[i]);
    //         }
    //
    //         return Variant.From(Many.Create(mapped, this), this.Engine);
    //     }
    //     finally
    //     {
    //         _variantArrayPool.Return(mapped, true);
    //     }
    // }
    //
    // private ValueTask<Variant> ExpandBundle(WeaveNode node, StepState parentState, IReadOnlyBundle<Variant> bundle)
    // {
    //     Debug.Assert(node.Handler?.Role == HandlerRole.Expander);
    //     throw new NotImplementedException();
    // }
    
    private PoolSet ChoosePoolSet()
    {
        _poolsetLease = this.Engine.PoolSets.TryLeaseUntracked();
        if (_poolsetLease.HasValue)
        {
            return _poolsetLease;
        }
        
        return new StaticPoolSet(this.Engine);
    }
}