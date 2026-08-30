using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

    private ConcurrentDictionary<ulong, bool> _declinedConverters = [];
    
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

    public IPlConverter? FindConverter(PlTypeDef source, PlTypeDef target)
    {
        var key = PlTypeDef.CombineIds(source, target);
        if (_cached.TryGetValue(key, out var cachedConverter))
            return cachedConverter;

        if (!this.IsConvertible(source, target))
            return null;

        if (!_converters.TryGetValue(key, out var candidates) || candidates.IsEmpty)
            return null;
        
        _cached[key] = candidates.Last();
        return candidates.Last();
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

    public Variant Convert(IWeaveContext context, scoped in Variant value, PlTypeDef target)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (!this.TryConvert(context, in value, target, out var converted))
            throw new PipeLoomException($"Failed to convert value to '{target.Name}' from '{value.ToString()}'");

        return converted;
    }

    public TTarget Convert<TSource, TTarget>(IWeaveContext context, TSource value)
    {
        if (!this.TryConvert<TSource, TTarget>(context, value, out var converted))
            throw new PipeLoomException($"Failed to convert value to '{typeof(TTarget).Name}' from '{typeof(TSource).Name}'");

        return converted;
    }
    
    public TTarget Convert<TTarget>(IWeaveContext context, scoped in Variant value)
    {
        if (!this.TryConvert<TTarget>(context, value, out var converted))
            throw new PipeLoomException($"Failed to convert value to '{typeof(TTarget).Name}' from '{value}'");

        return converted;
    }

    public bool TryConvert(IWeaveContext context, scoped in Variant value, PlTypeDef target, out Variant converted)
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

        if (source.Id == target.Id)
            return true; // identity, nothing to convert

        var key = PlTypeDef.CombineIds(source, target);

        if (!_cached.TryGetValue(key, out var converter))
        {
            if (!this.TryBuildConverter(source, target, out converter))
                return false;
            
            _cached[key] = converter;
        }
        
        converted = converter.Convert(context, in value);
        
        return true;
    }
    
    public bool TryConvert<TTarget>(IWeaveContext context, scoped in Variant value, out TTarget converted)
    {
        converted = default!;
        
        if (!this.TryConvert(context, in value, _engine.TypeOf<TTarget>(), out var vConverted))
        {
            return false;
        }
        
        converted = vConverted.Unpack<TTarget>();
        return true;
    }
    
    public bool TryConvert<TSource, TTarget>(IWeaveContext context, TSource value, out TTarget converted)
    {
        converted = default!;

        var source = Variant.From(value, _engine.TypeOf<TSource>());
        
        if (!this.TryConvert(context, in source, _engine.TypeOf<TTarget>(), out var vConverted))
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
        
        #pragma warning disable CS8601
        converter ??= this.GetOrCreateCrossGenericConverter(source, target);
        #pragma warning restore CS8601
        
        return converter is not null;
    }

    private PlConverter? FindDirectConverter(PlTypeDef source, PlTypeDef target)
    {
        var key = PlTypeDef.CombineIds(source, target);
        if (!_converters.TryGetValue(key, out var candidates) || candidates.IsEmpty)
            return null;

        return candidates[^1];
    }

    private PlConverter? GetOrCreateCrossGenericConverter(PlTypeDef source, PlTypeDef target)
    {
        // both have to be generic
        if (source is not IPlConstructed gSource || target is not IPlConstructed gTarget)
            return null;
        
        // both must have exactly a single generic arugment
        if (gSource.GenericArguments.Count != 1 || gTarget.GenericArguments.Count != 1)
            return null;
        
        var key = PlTypeDef.CombineIds(source, target);
        if (_declinedConverters.ContainsKey(key))
            return null;
        
        if (_converters.TryGetValue(key, out var converters) && !converters.IsEmpty)
        {
            return converters.Last();
        }
        
        if (gSource.GenericType == gTarget.GenericType)
        {
            var hMorphic = this.CreateHomomorphicConverter(
                gSource.GenericType,
                gSource.GenericArguments[0],
                gTarget.GenericArguments[0]);
            
            if (hMorphic is null)
            {
                _declinedConverters[key] = true;
            }

            return hMorphic;
        }

        var lifted = this.CreateLiftedGenericConverter(gSource, gTarget);
        if (lifted is null)
        {
            _declinedConverters[key] = true;
        }

        return lifted;
    }

    private PlConverter? CreateHomomorphicConverter(
        PlGenericType genericType,
        PlTypeDef sourceArg,
        PlTypeDef targetArg
        )
    {
        if (!genericType.SupportsHomomorphicConversion)
            return null;

        var reg = new ConverterRegistrator(_engine);
        
        return genericType.MakeGenericConverter(sourceArg, targetArg, reg);
    }

    private PlConverter? CreateLiftedGenericConverter(IPlConstructed gSource, IPlConstructed gTarget)
    {
        var vSource = gSource.GenericType.FindConstructedOfInner(_engine.WellKnown.Variant);
        var vTarget = gTarget.GenericType.FindConstructedOfInner(_engine.WellKnown.Variant);

        if (vSource is null || vTarget is null)
            return null;

        var vDirect = this.FindDirectConverter(vSource.SelfType, vTarget.SelfType);
        if (vDirect is null)
            return null;

        var sourceArg = gSource.GenericArguments.Single();
        if (sourceArg == _engine.WellKnown.Variant)
            return null; // inner conversion of Variant -> T is invalid
        
        var targetArg = gTarget.GenericArguments.Single();
        
        if (sourceArg == _engine.WellKnown.Variant && targetArg == _engine.WellKnown.Variant)
            return vDirect; // ?! this case shouldn't happen here, since direct converters have higher priority

        if (targetArg == _engine.WellKnown.Variant)
        {
            // A<T> -> B<Variant>
            // A<T> -> A<Variant> -> B<Variant>
            
            // T -> Variant is implicit, so this is lowered to the direct converter
            return vDirect;
        }
        
        // from here onward nor the source nor the target is Variant
        
        if (!sourceArg.IsConvertibleTo(targetArg))
            return null;

        var hSource = gSource.GenericType.FindConstructedOfInner(targetArg);
        var hTarget = gTarget.GenericType.FindConstructedOfInner(sourceArg);

        if (hSource is null && hTarget is null)
        {
            hSource = gSource.GenericType.FindOrCreateConstructedOfInner(targetArg);
            hTarget = hSource is null ? gTarget.GenericType.FindOrCreateConstructedOfInner(sourceArg) : null;
        }

        var hSourceConverter = hSource is not null ? this.FindConverter(gSource.SelfType, hSource.SelfType) : null;
        var hTargetConverter = hSourceConverter is null && hTarget is not null ? this.FindConverter(gTarget.SelfType, hTarget.SelfType) : null;

        if (hSourceConverter is null && hTargetConverter is null)
            return null; // at least one side needs an inner homomorphic converter, otherwise the conversion is nonsense

        return new PlLiftedGenericConverter(gSource, gTarget, _engine)
        {
            VDirect = vDirect,
            HSourceConverter = hSourceConverter,
            HTargetConverter = hTargetConverter
        };
    }
}