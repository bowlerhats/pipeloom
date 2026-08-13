using System;
using System.Runtime.CompilerServices;

namespace PipeLoom.Engine.Abstractions;

public interface IVariantDecomposable<TSelf>
    where TSelf: struct, IVariantDecomposable<TSelf>
{
    (object? reference, TSelf bare) DecomposeForVariant();
    static abstract TSelf ComposeFromPair(object? reference, TSelf bare);
}

public static class VariantDecomposeRegistrar<T>
    where T: struct, IVariantDecomposable<T>
{
    static VariantDecomposeRegistrar()
    {
        VariantDecomposableDispatch<T>.Decompose = static v => v.DecomposeForVariant();
        VariantDecomposableDispatch<T>.Compose = static (reference, bare) => T.ComposeFromPair(reference, bare);
    }
    
    // Work hard to avoid elision
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void EnsureRegistered(){ }
}

internal static class VariantDecomposableDispatch<T>
{
    public static Func<T, (object? reference, T bare)>? Decompose;
    public static Func<object?, T, T>? Compose;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDecomposable()
    {
        return Decompose is not null;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsComposable()
    {
        return Compose is not null;
    }
}