using System;
using System.Numerics;
using System.Numerics.Tensors;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Scalars;

internal static class CoreNumberConverters
{
    public static void AddStandardNumberConverters(ConverterRegistrator convertible)
    {
        // widening based on IEEE-754
        // only for frequently used types to balance noise/utility 
        
        //NativeConverter<byte, short>(in convertible);
        //NativeConverter<byte, ushort>(in convertible);
        NativeConverter<byte, int>(in convertible);
        //NativeConverter<byte, uint>(in convertible);
        NativeConverter<byte, long>(in convertible);
        //NativeConverter<byte, ulong>(in convertible);
        NativeConverter<byte, double>(in convertible);
        // NativeConverter<byte, decimal>(in convertible);
        
        // NativeConverter<short, int>(in convertible);
        // NativeConverter<short, long>(in convertible);
        // NativeConverter<short, double>(in convertible);
        // NativeConverter<short, decimal>(in convertible);
        
        // NativeConverter<ushort, int>(in convertible);
        // NativeConverter<ushort, uint>(in convertible);
        // NativeConverter<ushort, long>(in convertible);
        // NativeConverter<ushort, ulong>(in convertible);
        // NativeConverter<ushort, double>(in convertible);
        // NativeConverter<ushort, decimal>(in convertible);
        
        NativeConverter<int, long>(in convertible);
        NativeConverter<int, double>(in convertible);
        // NativeConverter<int, decimal>(in convertible);
        
        // NativeConverter<uint, long>(in convertible);
        // NativeConverter<uint, ulong>(in convertible);
        // NativeConverter<uint, double>(in convertible);
        // NativeConverter<uint, decimal>(in convertible);
        
        // NativeConverter<long, decimal>(in convertible);
        
        // NativeConverter<ulong, decimal>(in convertible);
    }

    public static void AddExtendedNumberConverters(ConverterRegistrator convertible)
    {
        NativeConverter<byte, short>(in convertible);
        NativeConverter<byte, ushort>(in convertible);
        // NativeConverter<byte, int>(in convertible);
        NativeConverter<byte, uint>(in convertible);
        // NativeConverter<byte, long>(in convertible);
        NativeConverter<byte, ulong>(in convertible);
        // NativeConverter<byte, double>(in convertible);
        NativeConverter<byte, decimal>(in convertible);
        
        NativeConverter<short, int>(in convertible);
        NativeConverter<short, long>(in convertible);
        NativeConverter<short, double>(in convertible);
        NativeConverter<short, decimal>(in convertible);
        
        NativeConverter<ushort, int>(in convertible);
        NativeConverter<ushort, uint>(in convertible);
        NativeConverter<ushort, long>(in convertible);
        NativeConverter<ushort, ulong>(in convertible);
        NativeConverter<ushort, double>(in convertible);
        NativeConverter<ushort, decimal>(in convertible);
        
        // NativeConverter<int, long>(in convertible);
        // NativeConverter<int, double>(in convertible);
        NativeConverter<int, decimal>(in convertible);
        
        NativeConverter<uint, long>(in convertible);
        NativeConverter<uint, ulong>(in convertible);
        NativeConverter<uint, double>(in convertible);
        NativeConverter<uint, decimal>(in convertible);
        
        NativeConverter<long, decimal>(in convertible);
        
        NativeConverter<ulong, decimal>(in convertible);
    }

    public static void AddTensorConverters(ConverterRegistrator convertible)
    {
        // TensorConverter<byte, short>(in convertible);
        // TensorConverter<byte, ushort>(in convertible);
        TensorConverter<byte, int>(in convertible);
        // TensorConverter<byte, uint>(in convertible);
        TensorConverter<byte, long>(in convertible);
        // TensorConverter<byte, ulong>(in convertible);
        TensorConverter<byte, double>(in convertible);
        
        // TensorConverter<short, int>(in convertible);
        // TensorConverter<short, long>(in convertible);
        // TensorConverter<short, double>(in convertible);
        //
        // TensorConverter<ushort, int>(in convertible);
        // TensorConverter<ushort, uint>(in convertible);
        // TensorConverter<ushort, long>(in convertible);
        // TensorConverter<ushort, ulong>(in convertible);
        // TensorConverter<ushort, double>(in convertible);
        
        TensorConverter<int, long>(in convertible);
        TensorConverter<int, double>(in convertible);
        
        // TensorConverter<uint, long>(in convertible);
        // TensorConverter<uint, ulong>(in convertible);
        // TensorConverter<uint, double>(in convertible);
    }

    public static void AddExtendedTensorConverters(ConverterRegistrator convertible)
    {
        TensorConverter<byte, short>(in convertible);
        TensorConverter<byte, ushort>(in convertible);
        // TensorConverter<byte, int>(in convertible);
        TensorConverter<byte, uint>(in convertible);
        // TensorConverter<byte, long>(in convertible);
        TensorConverter<byte, ulong>(in convertible);
        // TensorConverter<byte, double>(in convertible);
        
        TensorConverter<short, int>(in convertible);
        TensorConverter<short, long>(in convertible);
        TensorConverter<short, double>(in convertible);
        
        TensorConverter<ushort, int>(in convertible);
        TensorConverter<ushort, uint>(in convertible);
        TensorConverter<ushort, long>(in convertible);
        TensorConverter<ushort, ulong>(in convertible);
        TensorConverter<ushort, double>(in convertible);
        
        // TensorConverter<int, long>(in convertible);
        // TensorConverter<int, double>(in convertible);
        
        TensorConverter<uint, long>(in convertible);
        TensorConverter<uint, ulong>(in convertible);
        TensorConverter<uint, double>(in convertible);
    }

    private static void NativeConverter<T, U>(in ConverterRegistrator convertible)
        where T : struct, INumberBase<T> 
        where U : struct, INumberBase<U>
    {
        convertible.FromValue<T>().ToValue<U>().Using(Convert<T, U>);
    }

    private static U Convert<T, U>(IWeaveContext context, in T value)
        where T : struct, INumberBase<T>
        where U : struct, INumberBase<U>
    {
        return U.CreateChecked(value);
    }

    private static void TensorConverter<TInput, TOutput>(scoped in ConverterRegistrator convertible)
        where TInput : INumberBase<TInput>
        where TOutput : INumberBase<TOutput>
    {
        convertible
            .FromValue<Many<TInput>>()
            .ToValue<Many<TOutput>>()
            .Using(TensorCheckedConverter<TInput, TOutput>);
    }
    
    private static Many<TOutput> TensorCheckedConverter<TInput, TOutput>(IWeaveContext context, scoped in Many<TInput> input)
        where TInput : INumberBase<TInput>
        where TOutput : INumberBase<TOutput>
    {
        switch (input.Length)
        {
            case 0: return Many.Empty<TOutput>();
            case 1: return Many.Single(TOutput.CreateChecked(input[0]), context);
        }
        
        var pool = context.Pools.GetArrayPool<TOutput>();
        var buffer = pool.Rent(input.Length);
        try
        {
            var bufSpan = buffer.AsSpan(0, input.Length);
            TensorPrimitives.ConvertChecked(input.AsSpan(), bufSpan);
        
            return Many.Create(bufSpan, context);
        }
        finally
        {
            pool.Return(buffer);
        }
    }
}