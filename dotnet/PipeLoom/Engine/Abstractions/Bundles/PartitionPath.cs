using System;
using System.Collections.Generic;
using System.Diagnostics;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Pools;

namespace PipeLoom.Engine.Abstractions.Bundles;

public sealed class PartitionPath : IEquatable<PartitionPath>, IPoolReturnable
{
    public static readonly PartitionPath Default = new() { IsDefault = true };
    internal static readonly PartitionPath[] InlineDefaultPathArray = [Default];
    
    private readonly Variant[] _keys;

    public int Length { get; private set; }
    
    public bool IsDefault { get; private init; }
    
    public ReadOnlySpan<Variant> Keys => _keys.AsSpan(0, this.Length);
    
    internal PartitionPath()
    {
        _keys = new Variant[MagicNumbers.MaxBundlePartitionLevels];
    }

    internal void SetKeys(params ReadOnlySpan<Variant> keys)
    {
        if (this.IsDefault)
            throw new PipeLoomException("Default partition path cannot change");
        
        if (keys.Length > MagicNumbers.MaxBundlePartitionLevels)
            throw new PipeLoomException($"Partition path is too deep, max depth is {MagicNumbers.MaxBundlePartitionLevels}");

        this.Reset();

        if (keys.Length > 0)
        {
            keys.CopyTo(_keys);
            this.Length = keys.Length;
        }
    }

    internal void Reset()
    {
        if (this.IsDefault)
            return;
        
        Array.Clear(_keys);
        this.Length = 0;
    }
    
    public ReturnResult OnReturn(IObjectPool _)
    {
        Debug.Assert(!this.IsDefault);
        
        this.Reset();
        
        return ReturnResult.Ok();
    }
    
    public bool Equals(PartitionPath? other)
    {
        if (other is null || this.Length != other.Length || this.IsDefault != other.IsDefault)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (this.Length == 0)
            return false;
        
        for (var i = 0; i < this.Length; i++)
        {
            if (!_keys[i].Equals(other._keys[i]))
                return false;
        }
        
        return true;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is PartitionPath other && this.Equals(other));
    }
    
    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        return this.Length switch
        {
            // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
            0 => base.GetHashCode(),
            _ => Hash(this.IsDefault, this.Keys)
        };
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }

    public static bool operator ==(PartitionPath? left, PartitionPath? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        return left is not null && left.Equals(right);
    }

    public static bool operator !=(PartitionPath? left, PartitionPath? right)
    {
        if (ReferenceEquals(left, right))
            return false;
        
        return left is null || !left.Equals(right);
    }

    private static int HashArbitraryLengthKeys(int length, bool isDefault, ReadOnlySpan<Variant> keys)
    {
        Debug.Assert(length > 0);
        
        var res = HashCode.Combine(length, isDefault);
        for (var i = 0; i < length; i++)
        {
            res = HashCode.Combine(res, keys[i]);
        }

        return res;
    }

    private static int Hash(bool isDefault, ReadOnlySpan<Variant> keys)
    {
        var length = keys.Length;

        return length switch
        {
            0 => throw new ArgumentOutOfRangeException(nameof(keys), "Non-zero length keys expected"),
            1 => HashCode.Combine(length, isDefault, keys[0]),
            2 => HashCode.Combine(length, isDefault, keys[0], keys[1]),
            3 => HashCode.Combine(length, isDefault, keys[0], keys[1], keys[2]),
            4 => HashCode.Combine(length, isDefault, keys[0], keys[1], keys[2], keys[3]),
            5 => HashCode.Combine(length, isDefault, keys[0], keys[1], keys[2], keys[3], keys[4]),
            6 => HashCode.Combine(length, isDefault, keys[0], keys[1], keys[2], keys[3], keys[4], keys[5]),
            _ => HashArbitraryLengthKeys(length, isDefault, keys)
        };
    }

    public sealed class BoundPathComparer : IEqualityComparer<PartitionPath>, IAlternateEqualityComparer<ReadOnlySpan<Variant>, PartitionPath>
    {
        public static readonly BoundPathComparer Instance = new();
        
        public bool Equals(PartitionPath? x, PartitionPath? y)
        {
            return x?.Equals(y) ?? y is null;
        }

        public int GetHashCode(PartitionPath obj)
        {
            return obj.GetHashCode();
        }

        public bool Equals(ReadOnlySpan<Variant> alternate, PartitionPath other)
        {
            var length = alternate.Length;
            
            if (length == 0)
                throw new NotSupportedException("Zero-length lookup keys (aka default partition) are not supported");
            
            
            if (other.IsDefault || length != other.Length)
                return false;
            
            var otherKeys = other.Keys;
            
            Debug.Assert(otherKeys.Length == length);
            
            for (var i = 0; i < length; i++)
            {
                if (!alternate[i].Equals(otherKeys[i]))
                    return false;
            }

            return true;
        }

        public int GetHashCode(ReadOnlySpan<Variant> alternate)
        {
            return Hash(false, alternate);
        }

        public PartitionPath Create(ReadOnlySpan<Variant> alternate)
        {
            throw new NotSupportedException("Only PartitionPaths can be inserted to respect pooling and engine semantics");
        }
    }
}