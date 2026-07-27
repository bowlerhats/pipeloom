using System;
using System.Buffers;
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

    public WeaveContext(PipeLoomEngine engine, WeavePlan plan)
    {
        this.Engine = engine;
        this.Plan = plan;
        
        this.Pools = this.GrabPoolSet();
        _statePool = this.Pools.GetObjectPool<StepState>(_ => new StepState(), MagicNumbers.StepStatePoolSize);
        _variantPool = this.Pools.GetArrayPool<Variant>();
    }

    public void Dispose()
    {
        this.Pools.Return();
    }

    public ValueTask<Variant> Step()
    {
        return this.Step(this.Plan.RootNode, null, null);
    }

    private ValueTask<Variant> Step(WeaveNode node, StepState parentState)
    {
        var carry = parentState.Carry;
        if (carry.IsUndefined)
        {
            return this.Step(node, parentState, null);
        }
        
        if (node.Handler is null)
            throw new PipeLoomException($"Missing operator handler for '{node.OperatorName}'");

        var role = node.Handler.Role;
        if (role == HandlerRole.None)
        {
            return this.Step(node, parentState, null);
        }

        if (carry.TryUnpack<IReadOnlyBundle>(out var carriedBundle, reinterpret: true))
        {
            var bundle = carriedBundle.As<Variant>();
            
            if (role == HandlerRole.Bundler)
            {
                return this.BundleStep(node, parentState, bundle);
            }
            
            
            
        }
        
        return node.Handler.Role switch
        {
            HandlerRole.None => this.Step(node, parentState, null),
            // HandlerRole.Mapper => this.MapStep(node, parentState, parentState.Carry),
            // HandlerRole.Transformer => this.TransformStep(node, parentState, parentState.Carry),
            // HandlerRole.Reducer => this.ReduceStep(node, parentState, parentState.Carry),
            // HandlerRole.Expander => this.ExpandStep(node, parentState, parentState.Carry),
            // HandlerRole.Bundler => this.BundleStep(node, parentState, parentState.Carry),
            _ => throw new PipeLoomException("Unknown handler role")
        };
    }

    private async ValueTask<Variant> Step(WeaveNode node, StepState? parentState, Variant? @implicit)
    {
        if (node.Handler is null)
            throw new PipeLoomException($"Missing operator handler for '{node.OperatorName}'");
        
        var childrenCoount = node.Children.Count;
        
        var state = _statePool.Rent();
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
                    await this.Step(child, state);
                }
                else
                {
                    var argType = node.Handler.Signature.ArgumentTypes[i];
                    if (argType is PlDetached)
                    {
                        throw new NotImplementedException();
                    }
                    
                    var childOutput = await this.Step(child, state);
                
                    if (child.IsArgument)
                    {
                        argBuffer[argPos++] = childOutput;
                    }
                }
            }

            ReadOnlyMemory<Variant> arguments = argPos > 0 ? argBuffer.AsMemory(0, argPos) : Memory<Variant>.Empty;
            return await node.Handler.Adapter.Call(state, in arguments);
        }
        finally
        {
            state.Unbind();
            _statePool.Return(state);
            
            _variantPool.Return(argBuffer, true);
        }
    }

    private ValueTask<Variant> MapStep(WeaveNode node, StepState parentState, IReadOnlyBundle<Variant> carry)
    {
        throw new NotImplementedException();
    }

    private ValueTask<Variant> TransformStep(WeaveNode node, StepState parentState, IReadOnlyBundle<Variant> carry)
    {
        throw new NotImplementedException();
    }
    
    private ValueTask<Variant> ReduceStep(WeaveNode node, StepState parentState, IReadOnlyBundle<Variant> carry)
    {
        throw new NotImplementedException();
    }
    
    private ValueTask<Variant> ExpandStep(WeaveNode node, StepState parentState, IReadOnlyBundle<Variant> carry)
    {
        throw new NotImplementedException();
    }
    
    private ValueTask<Variant> BundleStep(WeaveNode node, StepState parentState, IReadOnlyBundle<Variant> carry)
    {
        throw new NotImplementedException();
    }
    
    private PoolSet GrabPoolSet()
    {
        throw new NotImplementedException();
        
        // if (this.Engine.PoolSets.TryRent(out var poolSet))
        //     return poolSet;
        //
        // return new StaticPoolSet();
    }
}