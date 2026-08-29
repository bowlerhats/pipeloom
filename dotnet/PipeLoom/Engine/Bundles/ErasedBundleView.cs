using System.Collections.Generic;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Bundles;

namespace PipeLoom.Engine.Bundles;

internal sealed class ErasedBundleView<T> : IErasedBundleView
{
    public IBundle Bundle => _bundle;

    private readonly Bundle<T> _bundle;

    public ErasedBundleView(Bundle<T> bundle)
    {
        _bundle = bundle;
    }
    
    public Many<Variant> GetErasedLeaf(PartitionPath path)
    {
        var leaf = _bundle.GetLeaf(path);
        
        return _bundle.Context.Engine.Conversions.Convert<Many<T>, Many<Variant>>(_bundle.Context, leaf);
    }

    public void SetLeaf(PartitionPath path, Many<Variant> leaf)
    {
        var converted = _bundle.Context.Engine.Conversions.Convert<Many<Variant>, Many<T>>(_bundle.Context, leaf);

        _bundle.SetLeaf(path, converted);
    }

    public void SetLeaf(PartitionPath path, Variant singularLeaf)
    {
        if (!singularLeaf.TryUnpack<T>(out var converted))
        {
            converted = _bundle.Context.Engine.Conversions.Convert<T>(_bundle.Context, singularLeaf);
        }
        
        _bundle.SetLeaf(path, converted);
    }

    public Many<Variant> Flatten()
    {
        // todo: optimize

        // temporary, just for testing
        using var readLatch = _bundle.Latch();
        
        var itemCount = _bundle.Items.Count;
        var res = new List<Variant>(itemCount);
        foreach (var item in _bundle.Items)
        {
            var packed = Variant.From(item, _bundle.ItemType);
            res.Add(packed);
        }

        return Many.Wrap(res);
    }

    public void Add(Variant item)
    {
        if (!item.TryUnpack<T>(out var converted))
        {
            converted = _bundle.Context.Engine.Conversions.Convert<T>(_bundle.Context, item);
        }
        
        _bundle.Add(converted);
    }

    public bool TryAdd(Variant item)
    {
        if (!item.TryUnpack<T>(out var converted))
        {
            converted = _bundle.Context.Engine.Conversions.Convert<T>(_bundle.Context, item);
        }
        
        return _bundle.TryAdd(converted);
    }

    public ValueTask AddAsync(Variant item)
    {
        if (!item.TryUnpack<T>(out var converted))
        {
            converted = _bundle.Context.Engine.Conversions.Convert<T>(_bundle.Context, item);
        }
        
        return _bundle.AddAsync(converted);
    }

    public ValueTask<bool> TryAddAsync(Variant item)
    {
        if (!item.TryUnpack<T>(out var converted))
        {
            converted = _bundle.Context.Engine.Conversions.Convert<T>(_bundle.Context, item);
        }
        
        return _bundle.TryAddAsync(converted);
    }
}