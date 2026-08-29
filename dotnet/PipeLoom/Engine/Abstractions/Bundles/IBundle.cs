using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipeLoom.Engine.Abstractions.Bundles;

public interface IBundle
{
    IWeaveContext Context { get; }
    
    IPipeLoomEngine Engine { get; }
    PlTypeDef ItemType { get; }
    
    IErasedBundleView Erased { get; }
    
    IEnumerable<PartitionPath> Paths { get; }
    
    int ItemCount { get; }
    
    bool Repartition(bool allowCollapse = false);
    ValueTask<bool> RepartitionAsync(bool allowCollapse = false);
    
    Variant PackAsVariant();
}

public interface IBundle<T> : IBundle
{
    IBundlePartitions<T> Partitions { get; }
    
    IBundleItems<T> Items { get; }
    
    Many<T> this[PartitionPath path] { get; set; }
    
    Many<T> SetLeaf(PartitionPath path, Many<T> leaf);
    Many<T> SetLeaf(PartitionPath path, T singularLeaf);

    IReadOnlyBundle<T> AsReadOnly();
    
    Many<T> Flatten();

    void Add(T item);
    // void Add(T item, PartitionPath path);
    // void Add(T item, params Variant[] path);
    bool TryAdd(T item);
    ValueTask AddAsync(T item);
    ValueTask<bool> TryAddAsync(T item);

    IBundle<TOther> ConvertTo<TOther>(Func<T, TOther> converterFunc);
    IBundle<TOther> ConvertTo<TOther, TState>(TState state, Func<TState, T, TOther> converterFunc);
}
