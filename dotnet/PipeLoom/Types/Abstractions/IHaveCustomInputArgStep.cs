using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Abstractions;

internal interface IPlCustomInputArgProvider
{
    // LATER: Make this interface part of the fusing process
    // Node should have a set of pre-converters, In essence this is a preconverter/pre-decider
    // And the deciders should be orthogonal to the type system, not just part of it.
    // This interface now is strictly because of Detached<T>
    
    bool TryProvide(IStepState state, int childIndex, out Variant providedInputArg);
}