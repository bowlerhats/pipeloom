using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine.Abstractions;

// Sequential here is not strictly needed because TResult is not part of the struct.
// Otherwise it is advised to use it for stable bliting. 
[StructLayout(LayoutKind.Sequential)]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.AllConstructors)]
public readonly struct Detached<TResult> : IEquatable<Detached<TResult>>, IVariantDecomposable<Detached<TResult>>
{
    // private readonly StepState _state;
    // private readonly int _childIndex;

    private readonly IWeaveNode _node;

    public IWeaveNode Node => _node;// _state.Node.Children[_childIndex];

    public Detached(IWeaveNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        
        _node = node;
    }
    
    // internal Detached(StepState state, int childIndex)
    // {
    //     ArgumentNullException.ThrowIfNull(state);
    //     ArgumentOutOfRangeException.ThrowIfNegative(childIndex);
    //
    //     if (state.Node.Children.Count <= childIndex)
    //         throw new IndexOutOfRangeException("Child index exceeds Node's children bounds");
    //
    //     _state = state;
    //     _childIndex = childIndex;
    // }
    //
    // private Detached(int childIndex)
    // {
    //     ArgumentOutOfRangeException.ThrowIfNegative(childIndex);
    //     
    //     _state = null!;
    //     _childIndex = childIndex;
    // }

    // public ValueTask<TResult> Step()
    // {
    //     return _state.Step(this);
    // }
    //
    // public ValueTask<TResult> Step<TCarry>(TCarry carry)
    // {
    //     return _state.Step(this, carry);
    // }
    

    #region Equality
    
    public bool Equals(Detached<TResult> other)
    {
        return Equals(_node, other._node);
    }

    public override bool Equals(object? obj)
    {
        return obj is Detached<TResult> other && this.Equals(other);
    }

    public override int GetHashCode()
    {
        return _node.GetHashCode();
    }
    
    public static bool operator ==(Detached<TResult> left, Detached<TResult> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Detached<TResult> left, Detached<TResult> right)
    {
        return !left.Equals(right);
    }
    
    #endregion
    
    
    #region Decomposable
    
    static Detached()
    {
        VariantDecomposeRegistrar<Detached<TResult>>.EnsureRegistered();
    }
    
    public (object? reference, Detached<TResult> bare) DecomposeForVariant()
    {
        if (_node is null)
            throw new PipeLoomException("Detached nodes need a proper node");
        
        return (_node, default);
    }

    public static Detached<TResult> ComposeFromPair(object? reference, Detached<TResult> _)
    {
        ArgumentNullException.ThrowIfNull(reference);

        return new Detached<TResult>((IWeaveNode)reference);
    }

    #endregion
}