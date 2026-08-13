using System.Numerics;
using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Scalars;

internal static class CoreNumberConverters
{
    public static void AddStandardNumberConverters(ConverterRegistrator convertible)
    {
        // widening based on IEEE-754
        // only for widely used types to balance noise/utility 
        
        NativeConverter<byte, short>(in convertible);
        NativeConverter<byte, ushort>(in convertible);
        NativeConverter<byte, int>(in convertible);
        NativeConverter<byte, uint>(in convertible);
        NativeConverter<byte, long>(in convertible);
        NativeConverter<byte, ulong>(in convertible);
        NativeConverter<byte, double>(in convertible);
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
        
        NativeConverter<int, long>(in convertible);
        NativeConverter<int, double>(in convertible);
        NativeConverter<int, decimal>(in convertible);
        
        NativeConverter<uint, long>(in convertible);
        NativeConverter<uint, ulong>(in convertible);
        NativeConverter<uint, double>(in convertible);
        NativeConverter<uint, decimal>(in convertible);
        
        NativeConverter<long, decimal>(in convertible);
        
        NativeConverter<ulong, decimal>(in convertible);
    }

    private static void NativeConverter<T, U>(in ConverterRegistrator convertible)
        where T : struct, INumberBase<T> 
        where U : struct, INumberBase<U>
    {
        convertible.FromValue<T>().ToValue<U>().Using(Convert<T, U>);
    }

    private static U Convert<T, U>(in T value)
        where T : struct, INumberBase<T>
        where U : struct, INumberBase<U>
    {
        return U.CreateChecked(value);
    }
}