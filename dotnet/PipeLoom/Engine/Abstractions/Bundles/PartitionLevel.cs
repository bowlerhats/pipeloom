using System;

namespace PipeLoom.Engine.Abstractions.Bundles;

public readonly struct PartitionLevel : IEquatable<PartitionLevel>
{
    public static readonly PartitionLevel Empty = default;
    
    private readonly int _depth;
    private readonly PlTypeDef _keyType;
    private readonly string? _name;
    private readonly IBundlePartitioner? _partitioner;

    // ReSharper disable ConvertToAutoProperty
    // ReSharper disable ConvertToAutoPropertyWhenPossible
    public int Depth => _depth;
    public PlTypeDef KeyType => _keyType;
    public string? Name => _name;
    public IBundlePartitioner? Partitioner => _partitioner;
    // ReSharper restore ConvertToAutoPropertyWhenPossible
    // ReSharper restore ConvertToAutoProperty

    public bool HasPartitioner => _partitioner is not null;

    public PartitionLevel(int depth, PlTypeDef keyType, IBundlePartitioner? partitioner = null, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(keyType);
        ArgumentOutOfRangeException.ThrowIfNegative(depth);
        
        _depth = depth;
        _keyType = keyType;
        _partitioner = partitioner;
        _name = name;
    }

    public bool Equals(PartitionLevel other)
    {
        return _depth == other._depth && Equals(_keyType, other._keyType);
    }

    public override bool Equals(object? obj)
    {
        return obj is PartitionLevel other && this.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_depth, _keyType);
    }

    public static bool operator ==(PartitionLevel left, PartitionLevel right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PartitionLevel left, PartitionLevel right)
    {
        return !(left == right);
    }
}