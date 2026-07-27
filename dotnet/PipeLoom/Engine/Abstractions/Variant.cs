using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PipeLoom.Engine.Abstractions;

// Keep at max 64 bytes to fit in a cache line
[StructLayout(LayoutKind.Sequential, Size = 64)]
public readonly struct Variant : IEquatable<Variant>
{
    private static object UndefinedMarker = new();
    public static readonly Variant Undefined = new (null, typeof(object), UndefinedMarker);
    
    private readonly bool _isReference;
    private readonly object? _reference;
    private readonly Type? _type;
    private readonly object? _tag;
    private readonly PackedValue _packed;

    // ReSharper disable ConvertToAutoPropertyWhenPossible
    // ReSharper disable ConvertToAutoProperty
    public bool IsReference => _isReference;
    public Type? UnderlyingType => _type;
    public object? Tag => _tag;
    public object? Reference => _reference;
    // ReSharper restore ConvertToAutoProperty
    // ReSharper restore ConvertToAutoPropertyWhenPossible

    public bool IsUndefined => ReferenceEquals(_tag, UndefinedMarker);

    private Variant(object? reference, Type type, object? tag)
    {
        _isReference = true;
        _reference = reference;
        _type = type;
        _tag = tag;
        _packed = default;
    }

    private Variant(PackedValue packed, Type type, object? tag)
    {
        _isReference = false;
        _reference = null;
        _type = type;
        _tag = tag;
        _packed = packed;
    }

    private Variant(scoped in Variant other, object? tag)
    {
        _isReference = other._isReference;
        _reference = other._reference;
        _type = other._type;
        _tag = tag ?? other._tag;
        _packed = other._packed;
    }

    [OverloadResolutionPriority(1)]
    public static Variant From(Variant value, object? tag = null)
    {
        return new Variant(value, tag);
    }

    public static Variant From<T>(IPipeLoomEngine engine, T value)
    {
        var type = engine.TypeOf<T>();
        return From(value, type);
    }
    
    public static Variant From<T>(T value, object? tag = null)
    {
        if (typeof(T) == typeof(Variant))
        {
            ref var v = ref Unsafe.As<T, Variant>(ref value);

            return new Variant(v, tag);
        }
        
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>() || Unsafe.SizeOf<T>() > 32)
        {
            return new Variant(value, typeof(T), tag);
        }
        
        PackedValue packed = default;
        
        Debug.Assert(Unsafe.SizeOf<T>() <= 32);
        Unsafe.WriteUnaligned(ref Unsafe.As<long, byte>(ref packed._0), value);
        
        return new Variant(packed, typeof(T), tag);
    }

    [OverloadResolutionPriority(1)]
    public Variant Unpack(bool reinterpret = false)
    {
        return this;
    }

    public T Unpack<T>(bool reinterpret = false)
    {
        if (typeof(T) == typeof(Variant))
        {
            var self = this;
            
            return Unsafe.As<Variant, T>(ref self);
        }
        
        if (!reinterpret && _type != typeof(T))
            throw new InvalidCastException($"Variant contains {_type}, cannot unpack as {typeof(T)}");

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>() || Unsafe.SizeOf<T>() > 32)
        {
            if (!_isReference)
                throw new InvalidCastException($"Tried to cast a value typed Variant ({_type}) to a reference type ({typeof(T)})");
            
            return (T)_reference!;
        }

        if (_isReference)
            return (T)_reference!;
        
        Debug.Assert(Unsafe.SizeOf<T>() <= 32);
        return Unsafe.ReadUnaligned<T>(ref Unsafe.As<long, byte>(ref Unsafe.AsRef(in _packed._0)));
    }
    
    [OverloadResolutionPriority(1)]
    public bool TryUnpack(out Variant value, bool reinterpret = false)
    {
        value = this;
        return true;
    }
    
    public bool TryUnpack<T>(out T value, bool reinterpret = false)
    {
        if (typeof(T) == typeof(Variant))
        {
            var self = this;
            value = Unsafe.As<Variant, T>(ref self);
            return true;
        }
        
        value = default!;
        
        if (!reinterpret && _type != typeof(T))
            return false;
        
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>() || Unsafe.SizeOf<T>() > 32)
        {
            return _isReference && TryCast(_reference, out value);
        }

        if (_isReference)
        {
            if (!TryCast(_reference, out value))
                return false;
        }
        else
        {
            Debug.Assert(Unsafe.SizeOf<T>() <= 32);
            value = Unsafe.ReadUnaligned<T>(ref Unsafe.As<long, byte>(ref Unsafe.AsRef(in _packed._0)));
        }

        return true;
    }

    public Variant UncheckedCastAs(Type type, object? tag = null)
    {
        return this.IsReference
            ? new Variant(_reference, type, tag)
            : new Variant(_packed, type, tag);
    }

    public bool Equals(Variant other)
    {
        if (_type != other._type)
            return false;

        if (this.IsUndefined || other.IsUndefined)
            return false;
        
        if (_isReference)
            return other._isReference && Equals(_reference, other._reference);
        
        return !other._isReference && _packed.Equals(other._packed);
    }

    public override bool Equals(object? obj)
    {
        return obj is Variant other && this.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_isReference, _reference, _type, _packed);
    }
    
    public static bool operator ==(Variant left, Variant right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Variant left, Variant right)
    {
        return !(left == right);
    }

    private static bool TryCast<T>(object? value, out T casted)
    {
        if (value is T v)
        {
            casted = v;
            return true;
        }

        casted = default!;
        return false;
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    private struct PackedValue : IEquatable<PackedValue>
    {
        [FieldOffset(0)]  public long _0;
        [FieldOffset(8)]  public long _1;
        [FieldOffset(16)] public long _2;
        [FieldOffset(24)] public long _3;

        public bool Equals(PackedValue other)
        {
            return _0 == other._0 && _1 == other._1 && _2 == other._2 && _3 == other._3;
        }

        public override bool Equals(object? obj)
        {
            return obj is PackedValue other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_0, _1, _2, _3);
        }
    }
}