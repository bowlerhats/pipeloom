using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PipeLoom.Engine.Abstractions;

[Flags]
internal enum VariantFlags
{
    None = 0,
    HasReference = 1 << 0,
    HasPackedValue = 1 << 1,
    IsDecomposed = 1 << 2
}

// Keep at max 64 bytes to fit in a cache line
[StructLayout(LayoutKind.Sequential, Size = 64)]
public readonly struct Variant : IEquatable<Variant>
{
    public static readonly Variant Undefined = new();
    
    private readonly VariantFlags _flags;
    private readonly object? _reference;
    private readonly Type? _type;
    private readonly object? _tag;
    private readonly PackedValue _packed;

    // ReSharper disable ConvertToAutoPropertyWhenPossible
    // ReSharper disable ConvertToAutoProperty
    public Type? UnderlyingType => _type;
    public object? Tag => _tag;
    public object? Reference => _reference;
    // ReSharper restore ConvertToAutoProperty
    // ReSharper restore ConvertToAutoPropertyWhenPossible
    
    public bool IsPureReference => this.HasReference && !this.HasPackedValue;
    public bool IsPureValue => !this.HasReference && this.HasPackedValue;

    public bool IsUndefined
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _flags == VariantFlags.None;
    }

    private bool HasReference
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (_flags & VariantFlags.HasReference) != 0;
    }

    private bool HasPackedValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (_flags & VariantFlags.HasPackedValue) != 0;
    }

    private bool IsDecomposed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (_flags & VariantFlags.IsDecomposed) != 0;
    }

    public Variant()
    {
        _flags = VariantFlags.None;
        AssertValidFlags(_flags);
    }
    
    private Variant(object? reference, Type type, object? tag)
    {
        _flags = VariantFlags.HasReference;
        AssertValidFlags(_flags);
        
        _reference = reference;
        _type = type;
        _tag = tag;
        _packed = default;
    }

    private Variant(PackedValue packed, Type type, object? tag)
    {
        _flags = VariantFlags.HasPackedValue;
        AssertValidFlags(_flags);
        
        _reference = null;
        _type = type;
        _tag = tag;
        _packed = packed;
    }

    private Variant(scoped in Variant other, object? tag)
    {
        _flags = other._flags;
        AssertValidFlags(_flags);
        
        _reference = other._reference;
        _type = other._type;
        _tag = tag ?? other._tag;
        _packed = other._packed;
    }
    
    private Variant(object? reference, PackedValue packed, Type type, object? tag, bool decomposed)
    {
        _flags = decomposed ? VariantFlags.IsDecomposed : VariantFlags.None;
        _flags |= VariantFlags.HasReference | VariantFlags.HasPackedValue;
        AssertValidFlags(_flags);
        
        _reference = reference;
        _type = type;
        _tag = tag;
        _packed = packed;
    }

    [OverloadResolutionPriority(1)]
    public static Variant From(Variant value, object? tag = null)
    {
        return new Variant(value, tag);
    }

    public static Variant From<T>(T value, IPipeLoomEngine engine)
    {
        var type = engine.TypeOf<T>();
        return From(value, type);
    }
    
    public static Variant From<T>(T value, object? tag = null)
    {
        if (typeof(T) == typeof(Variant))
        {
            ref readonly var v = ref Unsafe.As<T, Variant>(ref value);

            return new Variant(v, tag);
        }
        
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if (!typeof(T).IsValueType || Unsafe.SizeOf<T>() > 32 || !VariantDecomposableDispatch<T>.IsDecomposable())
            {
                return new Variant(value, typeof(T), tag);
            }
            
            var (reference, bare) = VariantDecomposableDispatch<T>.Decompose!(value);
                
            PackedValue packedBare = default;
                
            Debug.Assert(Unsafe.SizeOf<T>() <= 32);
            Unsafe.WriteUnaligned(ref Unsafe.As<long, byte>(ref packedBare._0), bare);
                
            return new Variant(reference, packedBare, typeof(T), tag, true);
        }
        
        if (Unsafe.SizeOf<T>() > 32)
        {
            // Too big, force boxing to heap
            return new Variant(value, typeof(T), tag);
        }
        
        PackedValue packed = default;
        
        Debug.Assert(Unsafe.SizeOf<T>() <= 32);
        Unsafe.WriteUnaligned(ref Unsafe.As<long, byte>(ref packed._0), value);
        
        return new Variant(packed, typeof(T), tag);
    }

    [OverloadResolutionPriority(1)]
    public Variant Unpack()
    {
        return this;
    }

    public T Unpack<T>()
    {
        if (typeof(T) == typeof(Variant))
        {
            var self = this;
            
            return Unsafe.As<Variant, T>(ref self);
        }
        
        if (_type != typeof(T))
            throw new InvalidCastException($"Variant contains {_type}, cannot unpack as {typeof(T)}");

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if (!typeof(T).IsValueType || Unsafe.SizeOf<T>() > 32 || !VariantDecomposableDispatch<T>.IsComposable())
            {
                if (!this.IsPureReference)
                    throw new InvalidCastException($"Tried to cast a value typed or decomposed Variant ({_type}) to a reference type ({typeof(T)})");
                
                return (T)_reference!;
            }

            if (!this.IsDecomposed || !this.HasReference || !this.HasPackedValue)
                throw new InvalidOperationException("Decomposed Variant with captured reference and value is expected");
            
            Debug.Assert(Unsafe.SizeOf<T>() <= 32);
            var bare = Unsafe.ReadUnaligned<T>(ref Unsafe.As<long, byte>(ref Unsafe.AsRef(in _packed._0)));

            return VariantDecomposableDispatch<T>.Compose!(_reference, bare);
        }

        if (!this.IsPureValue)
            throw new InvalidCastException("Tried to read a reference holding Variant into a value typed result");
       
        Debug.Assert(Unsafe.SizeOf<T>() <= 32);
        return Unsafe.ReadUnaligned<T>(ref Unsafe.As<long, byte>(ref Unsafe.AsRef(in _packed._0)));
    }
    
    [OverloadResolutionPriority(1)]
    public bool TryUnpack(out Variant value)
    {
        value = this;
        return true;
    }
    
    public bool TryUnpack<T>(out T value)
    {
        if (typeof(T) == typeof(Variant))
        {
            var self = this;
            value = Unsafe.As<Variant, T>(ref self);
            return true;
        }
        
        value = default!;
        
        if (_type != typeof(T))
            return false;
        
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if (!typeof(T).IsValueType || Unsafe.SizeOf<T>() > 32 || !VariantDecomposableDispatch<T>.IsComposable())
            {
                return this.IsPureReference && TryCast(_reference, out value);
            }

            if (!this.IsDecomposed || !this.HasReference || !this.HasPackedValue)
                return false;
            
            Debug.Assert(Unsafe.SizeOf<T>() <= 32);
            var bare = Unsafe.ReadUnaligned<T>(ref Unsafe.As<long, byte>(ref Unsafe.AsRef(in _packed._0)));

            value = VariantDecomposableDispatch<T>.Compose!(_reference, bare);
            return true;
        }

        if (!this.IsPureValue)
            return false;
        
        Debug.Assert(Unsafe.SizeOf<T>() <= 32);
        value = Unsafe.ReadUnaligned<T>(ref Unsafe.As<long, byte>(ref Unsafe.AsRef(in _packed._0)));

        return true;
    }

    // public Variant UncheckedCastAs(Type type, object? tag = null)
    // {
    //     if (this.IsUndefined)
    //         throw new InvalidCastException("Undefined Variant cannot be cast to a type");
    //     
    //     return this.IsDecomposed
    //         ? new Variant(_reference, _packed, type, tag, true)
    //         : this.IsPureReference
    //             ? new Variant(_reference, type, tag)
    //             : this.IsPureValue
    //                 ? new Variant(_packed, type, tag)
    //                 // This state should be unreachable, refer to AssertValidFlags for more info
    //                 : throw new InvalidOperationException(
    //                     $"Variant is in an invalid state: {_flags}. This indicates a bug, not user error.");
    // }

    public static Variant VerbatimCopyUnsafe<TVariant>(in TVariant opaqueVariant)
    {
        if (typeof(TVariant) != typeof(Variant))
            throw new ArgumentException("Expected a Variant as input for verbatim copies");
        
        ref readonly var v = ref Unsafe.As<TVariant, Variant>(ref Unsafe.AsRef(in opaqueVariant));

        return new Variant(v, v.Tag);
    }

    public bool Equals(Variant other)
    {
        if (_flags != other._flags || _type != other._type)
            return false;

        if (this.IsUndefined)
            return false;

        var eq = true;
        
        if (this.HasReference)
        {
            Debug.Assert(other.HasReference);
            eq &= Equals(_reference, other._reference);
        }

        if (eq && this.HasPackedValue)
        {
            Debug.Assert(other.HasPackedValue);
            eq &= _packed.Equals(other._packed);
        }

        return eq;
    }

    public override bool Equals(object? obj)
    {
        return obj is Variant other && this.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_flags, _reference, _type, _packed);
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

    [Conditional("DEBUG")]
    private static void AssertValidFlags(VariantFlags flags)
    {
        // Single source of truth for the VariantFlags invariant.
        
        // !!! Keep in sync with UncheckedCastAs
        
        var isUndefined = flags == VariantFlags.None;

        if (isUndefined)
            return; // Implicitly true that if isUndefined then the others cannot be true
        
        var hasRef = (flags & VariantFlags.HasReference) != 0;
        var hasPacked = (flags & VariantFlags.HasPackedValue) != 0;
        var isDecomposed = (flags & VariantFlags.IsDecomposed) != 0;
        
        if (isDecomposed)
        {
            Debug.Assert(hasRef && hasPacked, $"Decomposed must have both ref and value. Flags: {flags}");
        }
        else
        {
            Debug.Assert(hasRef ^ hasPacked, $"Non-decomposed must have either ref or value. Flags: {flags}");
        }
    }

    public override string? ToString()
    {
        if (this.IsUndefined)
            return "Variant(Undefined)";

        if (this.Tag is PlTypeDef def)
        {
            var asString = def.VariantToStringForDebug(in this);
            if (!string.IsNullOrWhiteSpace(asString))
                return asString;
        }

        if (this.IsPureReference)
            return $"Variant({_reference?.ToString() ?? _type?.FullName ?? "?reference?"})";
        
        if (this.IsPureValue)
            return $"Variant({_type?.FullName ?? "?value?"})";

        if (this.IsDecomposed)
        {
            return $"Variant({_reference?.ToString() ?? "?reference?"}, {_type?.FullName ?? "?type?"})";
        }
        
        return base.ToString();
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