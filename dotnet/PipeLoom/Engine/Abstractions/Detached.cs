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
    private readonly StepState _state;
    private readonly int _childIndex;
    
    public IWeaveNode Node => _state.Node.Children[_childIndex];
    
    internal Detached(StepState state, int childIndex)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentOutOfRangeException.ThrowIfNegative(childIndex);

        if (state.Node.Children.Count <= childIndex)
            throw new IndexOutOfRangeException("Child index exceeds Node's children bounds");

        _state = state;
        _childIndex = childIndex;
    }

    private Detached(int childIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(childIndex);
        
        _state = null!;
        _childIndex = childIndex;
    }

    public ValueTask<TResult> Step()
    {
        return _state.Step(in this);
    }
    
    public ValueTask<TResult> Step<TCarry>(TCarry carry)
    {
        return _state.Step(in this, carry);
    }
    

    #region Equality
    
    public bool Equals(Detached<TResult> other)
    {
        return Equals(_state, other._state) && _childIndex == other._childIndex;
    }

    public override bool Equals(object? obj)
    {
        return obj is Detached<TResult> other && this.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_state, _childIndex);
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
        if (_state is null)
            throw new PipeLoomException("Detached nodes need a proper state");
        
        return (_state, new Detached<TResult>(_childIndex));
    }

    public static Detached<TResult> ComposeFromPair(object? reference, Detached<TResult> bare)
    {
        ArgumentNullException.ThrowIfNull(reference);

        return new Detached<TResult>((StepState)reference, bare._childIndex);
    }

    #endregion
}