using System;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Operators.Abstractions;

public static class PlOperatorDelegates
{
    public delegate Variant NullaryFunction();
    public delegate Variant NullaryFunctionWithStep(in WeaveStep step);
    public delegate ValueTask<Variant> NullaryFunctionAsync();
    public delegate ValueTask<Variant> NullaryFunctionAsyncWithStep(WeaveStep step);
    
    public delegate Variant UnaryFunction(in Variant arg1);
    public delegate Variant UnaryFunctionWithStep(in WeaveStep step, in Variant arg1);
    public delegate ValueTask<Variant> UnaryFunctionAsync(Variant arg1);
    public delegate ValueTask<Variant> UnaryFunctionAsyncWithStep(WeaveStep step, Variant arg1);
    
    public delegate Variant BinaryFunction(in Variant arg1, in Variant arg2);
    public delegate Variant BinaryFunctionWithStep(in WeaveStep step, in Variant arg1, in Variant arg2);
    public delegate ValueTask<Variant> BinaryFunctionAsync(Variant arg1, Variant arg2);
    public delegate ValueTask<Variant> BinaryFunctionAsyncWithStep(WeaveStep step, Variant arg1, Variant arg2);
    
    public delegate Variant TernaryFunction(in Variant arg1, in Variant arg2, in Variant arg3);
    public delegate Variant TernaryFunctionWithStep(in WeaveStep step, in Variant arg1, in Variant arg2, in Variant arg3);
    public delegate ValueTask<Variant> TernaryFunctionAsync(Variant arg1, Variant arg2, Variant arg3);
    public delegate ValueTask<Variant> TernaryFunctionAsyncWithStep(WeaveStep step, Variant arg1, Variant arg2, Variant arg3);
    
    public delegate Variant VariadicFunction(in ReadOnlyMemory<Variant> args);
    public delegate Variant VariadicFunctionWithStep(in WeaveStep step, in ReadOnlyMemory<Variant> args);
    public delegate ValueTask<Variant> VariadicFunctionAsync(ReadOnlyMemory<Variant> args);
    public delegate ValueTask<Variant> VariadicFunctionAsyncWithStep(WeaveStep step, ReadOnlyMemory<Variant> args);
}