using System;
using System.Buffers;
using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Engine;

internal readonly struct VariantRecaster<T> : IDisposable
{
    public readonly ReadOnlyMemory<T> Memory;
    
    private readonly T[] _rented;
    private readonly ArrayPool<T>? _pool;
        
    public VariantRecaster(ArrayPool<T> pool, Converter<Variant, T> converter, scoped in ReadOnlySpan<Variant> inputs)
    {
        var inputLength = inputs.Length;
        if (inputLength <= 0)
        {
            _rented = [];
            Memory = ReadOnlyMemory<T>.Empty;
            
            return;
        }
        
        _pool = pool;
        _rented = _pool.Rent(inputs.Length);

        for (var i = 0; i < inputLength; i++)
        {
            _rented[i] = converter(inputs[i]);
        }

        Memory = _rented.AsMemory(0, inputLength);
    }

    public void Dispose()
    {
        _pool?.Return(_rented, true);
    }
}