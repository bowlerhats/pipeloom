using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine.TypeConversions;

public delegate T VariantUnpacker<out T>(scoped in Variant v);
public delegate Variant VariantPacker<T>(in T v);

internal sealed class ConversionMap : IPlConversionMap
{
    private readonly PipeLoomEngine _engine;

    private ConcurrentDictionary<Type, Delegate> _toVariantCache = [];
    
    private ConcurrentDictionary<ulong, ImmutableList<PlConverter>> _converters = [];

    private ConcurrentDictionary<ulong, PlConverter> _cached = [];
    
    public ConversionMap(PipeLoomEngine engine)
    {
        _engine = engine;
    }

    public void Add(IEnumerable<PlTypeDef> typeDefs)
    {
        var registrator = new ConverterRegistrator(_engine);
        
        foreach (var def in typeDefs)
        {
            def.SetupConverters(registrator);
        }
    }

    public void Add(IEnumerable<Action<ConverterRegistrator>> registrations)
    {
        var registrator = new ConverterRegistrator(_engine);

        foreach (var registration in registrations)
        {
            registration(registrator);
        }
    }
    
    public void Add(PlTypeDef def)
    {
        var registrator = new ConverterRegistrator(_engine);
        
        def.SetupConverters(in registrator);
    }

    public void Add<TConverter>(TConverter converter)
        where TConverter : PlConverter
    {
        ArgumentNullException.ThrowIfNull(converter);
        
        if (converter.SourceType.Id == converter.TargetType.Id)
            throw new PipeLoomException("Self into self converters are not allowed");
        
        _converters.AddOrUpdate(converter.TypeId,
            static (_, conv) => [conv],
            static (_, prev, conv) => prev.Add(conv),
            converter
            );
    }

    public VariantPacker<T>? FindCustomVariantPacker<T>()
    {
        // TODO: Implement user defined packers 
        return null;
    }
    
    public Converter<T, Variant> ToVariant<T>()
    {
        if (typeof(T) == typeof(Variant))
        {
            // Variant to Variant should preserve identity
            return ConvIdentity;
        }
        
        if (_toVariantCache.TryGetValue(typeof(T), out var converter))
        {
            return (Converter<T, Variant>)converter;
        }

        var plType = _engine.TypeOf<T>();
        
        _toVariantCache[typeof(T)] = (Converter<T, Variant>)Conv;

        return Conv;
        
        static Variant ConvIdentity(T v) => Variant.From(v);
        Variant Conv(T v) => Variant.From(v, plType);
    }

    public VariantUnpacker<T>? FindCustomVariantUnpacker<T>()
    {
        // TODO: Implement user defined unpackers
        return null;
    }

    public bool IsConvertible(PlTypeDef from, PlTypeDef to)
    {
        if (from.Id == to.Id)
            return true; // identity

        if (to == _engine.WellKnown.Variant)
            return true; // everything is implicitly convertible to Variant

        var key = PlTypeDef.CombineIds(from, to);

        if (_cached.ContainsKey(key))
            return true;
        
        if (!this.TryBuildConverter(from, to, out var converter))
            return false;

        _cached[key] = converter;

        return true;
    }

    public Variant Convert(scoped in Variant value, PlTypeDef target)
    {
        return this.TryConvert(in value, target, out var converted)
            ? converted
            : throw new PipeLoomException($"Failed to convert value to '{target.Name}' from '{value.ToString()}'");
    }

    public bool TryConvert(scoped in Variant value, PlTypeDef target, out Variant converted)
    {
        converted = value;

        if (value.IsUndefined)
            // Only valid conversion is to Variant itself
            return target == _engine.WellKnown.Variant;

        if (value.Tag == target || target == _engine.WellKnown.Variant)
            return true; // identity/noop conversion

        var source = value.Tag as PlTypeDef ?? _engine.TypeMap.Infer(in value);

        if (source is null)
            return false; // Unknown captured value -> unsafe to attempt conversion

        var key = PlTypeDef.CombineIds(source, target);

        if (!_cached.TryGetValue(key, out var converter))
        {
            if (!this.TryBuildConverter(source, target, out converter))
                return false;
            
            _cached[key] = converter;
        }
        
        converted = converter.Convert(in value);
        
        return true;
    }
    
    public bool TryConvert<TTarget>(scoped in Variant value, out TTarget converted)
    {
        converted = default!;
        
        if (!this.TryConvert(in value, _engine.TypeOf<TTarget>(), out var vConverted))
        {
            return false;
        }
        
        converted = vConverted.Unpack<TTarget>();
        return true;
    }

    private bool TryBuildConverter(PlTypeDef source, PlTypeDef target, [MaybeNullWhen(false)] out PlConverter converter)
    {
        converter = null;
        
        if (source.Id == target.Id)
            return false; // No type should convert into itself
        
        // TODO: Find and build conversion chains
        
        converter = this.FindDirectConverter(source, target);
        
        return converter is not null;
    }

    private PlConverter? FindDirectConverter(PlTypeDef source, PlTypeDef target)
    {
        var key = PlTypeDef.CombineIds(source, target);
        if (!_converters.TryGetValue(key, out var candidates) || candidates.IsEmpty)
            return null;

        return candidates[^1];
    }
}