using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
// ReSharper disable InconsistentlySynchronizedField

namespace PipeLoom.Engine;

internal sealed class TypeMap
{
    public IEnumerable<PlTypeDef> TypeDefs => _typeDefsById.Values;
    
    private readonly PipeLoomEngine _engine;
    
    private readonly Lock _discoveryLock = new();
    
    private FrozenDictionary<Type, PlGenericType> _genericsByNative = FrozenDictionary<Type, PlGenericType>.Empty;
    
    private readonly ConcurrentDictionary<Type, PlTypeDef> _discoveredTypeDefsByType = [];
    private FrozenDictionary<Type, PlTypeDef> _typeDefsByType = FrozenDictionary<Type, PlTypeDef>.Empty;
    
    private readonly ConcurrentDictionary<int, PlTypeDef> _discoveredTypeDefsById = [];
    private FrozenDictionary<int, PlTypeDef> _typeDefsById = FrozenDictionary<int, PlTypeDef>.Empty;
    
    private readonly ConcurrentDictionary<Type, PlTypeDef[]> _discoveredByNativeType = [];
    private FrozenDictionary<Type, PlTypeDef[]> _byNativeType = FrozenDictionary<Type, PlTypeDef[]>.Empty;
    
    private readonly ConcurrentDictionary<Type, PlTypeDef> _resolveCache = [];
    
    public TypeMap(PipeLoomEngine engine, IEngineConfig config)
    {
        _engine = engine;
        
        this.Build(config);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PlTypeDef? Infer(scoped in Variant value, bool skipTag = true)
    {
        return this.TryInfer(in value, out var def, skipTag) ? def : null;
    }

    /// <summary>
    /// Tries to infer the typedef of a Variant checking in reverse fashion
    /// </summary>
    /// <remarks>
    /// Intentionally guessing the type in reverse from how the Variant should be checked
    /// </remarks>
    /// <param name="value">Variant to be checked</param>
    /// <param name="def">The inferred resulting type</param>
    /// <param name="skipTag">Skips strict tag check. Useful when want to ignore the explicit typedef and only do a pure inference</param>
    /// <returns>True, when the Variant is succesfully corresponding to an inferred typedef</returns>
    public bool TryInfer(scoped in Variant value, out PlTypeDef def, bool skipTag = true)
    {
        def = null!;

        if (value.IsUndefined)
            return false;
        
        if (value is { IsPureReference: true, Reference: not null }
            && this.TryGetClosestCommonAncestor(this.FindNativeCandidates(value.Reference.GetType()), out var common))
        {
            def = common;
            return true;
        }
        
        if (value.UnderlyingType is not null
            && this.TryGetClosestCommonAncestor(this.FindNativeCandidates(value.UnderlyingType), out common))
        {
            def = common;
            return true;
        }
        
        if (!skipTag && value.Tag is PlTypeDef vDef)
        {
            def = vDef;
            return true;
        }

        return false;
    }

    public T? Find<T>()
        where T : PlTypeDef
    {
        if (_typeDefsByType.TryGetValue(typeof(T), out var plType)
            || _discoveredTypeDefsByType.TryGetValue(typeof(T), out plType))
        {
            return (T)plType;
        }

        return null;
    }

    public PlGenericType? FindGeneric(Type nativeOpenGenericType)
    {
        return _genericsByNative.GetValueOrDefault(nativeOpenGenericType);
    }

    public IEnumerable<PlTypeDef> FindNativeCandidates(Type nativeType)
    {
        if (_byNativeType.TryGetValue(nativeType, out var candidates))
        {
            for (var i = 0; i < candidates.Length; i++)
            {
                yield return candidates[i];
            }
        }
        
        if (_discoveredByNativeType.TryGetValue(nativeType, out candidates))
        {
            for (var i = 0; i < candidates.Length; i++)
            {
                yield return candidates[i];
            }
        }
    }
    
    public PlTypeDef TypeOf<T>()
    {
        if (_resolveCache.TryGetValue(typeof(T), out var resolved))
            return resolved;

        lock (_discoveryLock)
        {
            return _resolveCache.GetOrAdd(typeof(T), static (t, f) => f.Resolve(t), this);
        }
    }

    public PlTypeDef TypeOf(Type type)
    {
        if (_resolveCache.TryGetValue(type, out var resolved))
            return resolved;

        lock (_discoveryLock)
        {
            return _resolveCache.GetOrAdd(type, static (t, f) => f.Resolve(t), this);
        }
    }
    
    public PlTypeDef Resolve(Type type)
    {
        var nativeCandidates = _byNativeType.GetValueOrDefault(type, []);
        var discoveredNativeCandidates = _discoveredByNativeType.GetValueOrDefault(type, []);
        var totalNativeCandidates = nativeCandidates.Length + discoveredNativeCandidates.Length;
        
        if (totalNativeCandidates > 0)
        {
            if (totalNativeCandidates == 1)
            {
                return nativeCandidates.Length == 1
                    ? nativeCandidates[0]
                    : discoveredNativeCandidates[0];
            }

            if (this.TryGetClosestCommonAncestor(
                    nativeCandidates.Concat(discoveredNativeCandidates),
                    out var commonAncestor))
            {
                return commonAncestor;
            }

            throw new PipeLoomException($"Ambigous resolution for '{type.Name}'");
        }

        if (type.IsGenericType && _genericsByNative.TryGetValue(type.GetGenericTypeDefinition(), out var genericType))
        {
            var constructed = this.ConstructGeneric(type, genericType);
            
            _engine.Conversions.Add(constructed);
            
            return constructed;
        }
        
        throw new PipeLoomException($"Unresolvable type '{type.FullName}'");
    }
    
    public void Compact()
    {
        if (_discoveredTypeDefsByType.IsEmpty)
            return;
        
        lock (_discoveryLock)
        {
            var mergerById = _typeDefsById.ToDictionary();
            foreach (var (key, value) in _discoveredTypeDefsById)
            {
                mergerById.Add(key, value);
            }

            var mergerByType = _typeDefsByType.ToDictionary();
            foreach (var (key, value) in _discoveredTypeDefsByType)
            {
                mergerByType.Add(key, value);
            }

            var mergerByNative = _byNativeType.ToDictionary();
            foreach (var (key, candidates) in _discoveredByNativeType)
            {
                var existing = mergerByNative.GetValueOrDefault(key, []);
                mergerByNative[key] = existing.Concat(candidates).Distinct().ToArray();
            }

            _typeDefsById = mergerById.ToFrozenDictionary();
            _typeDefsByType = mergerByType.ToFrozenDictionary();
            _byNativeType = mergerByNative.ToFrozenDictionary();
            
            _discoveredByNativeType.Clear();
            _discoveredTypeDefsByType.Clear();
            _discoveredTypeDefsById.Clear();
            
            _resolveCache.Clear();
        }
    }

    internal PlTypeDef ConstructGeneric(Type target, PlGenericType genericType)
    {
        var args = target.GetGenericArguments().Select(this.TypeOf).ToList();
        var constructed = genericType.ConstructGeneric(target, args);
        
        if (constructed is null)
            throw new PipeLoomException("Failed to construct from open generic");
        
        this.FindAndSetSuperset(constructed);

        lock (_discoveryLock)
        {
            var added = _discoveredTypeDefsByType.TryAdd(constructed.GetType(), constructed)
                        && _discoveredTypeDefsById.TryAdd(constructed.Id, constructed);

            if (added && !constructed.IsFloating)
            {
                var candidates = _discoveredByNativeType.GetValueOrDefault(constructed.NativeType, []);
                _discoveredByNativeType[constructed.NativeType] = [..candidates, constructed];
            }
            
            if (!added)
            {
                _discoveredTypeDefsByType.TryRemove(constructed.GetType(), out _);
                _discoveredTypeDefsById.TryRemove(constructed.Id, out _);
                
            }
            
            return constructed;
        }
    }
    
    private void Build(IEngineConfig config)
    {
        Dictionary<Type, PlTypeDef> typeDefs = [];
        Dictionary<Type, PlGenericType> genericsByNative = [];
        
        foreach (var factory in config.TypeFactories)
        {
            var def = factory(_engine);
            
            typeDefs.Add(def.GetType(), def);

            if (def is PlGenericType generic)
            {
                genericsByNative.Add(generic.NativeType, generic);
            }
        }

        _typeDefsByType = typeDefs.ToFrozenDictionary();
        _typeDefsById = typeDefs.ToFrozenDictionary(d => d.Value.Id, d => d.Value);
        _genericsByNative = genericsByNative.ToFrozenDictionary();
        _byNativeType = typeDefs
            .Where(d => !d.Value.IsFloating)
            .GroupBy(d => d.Value.NativeType)
            .ToFrozenDictionary(g => g.Key, g => g.Select(d => d.Value).ToArray());
        
        // Calculate supersets
        var superSet = new int[MagicNumbers.MaxSubsetPath];

        foreach (var (_, def) in typeDefs)
        {
            Array.Clear(superSet);

            this.FindAndSetSuperset(def, superSet);
        }
    }

    private void FindAndSetSuperset(PlTypeDef def, int[]? buffer = null)
    {
        var superset = buffer ?? new int[MagicNumbers.MaxSubsetPath];
        Debug.Assert(superset.Length >= MagicNumbers.MaxSubsetPath);
            
        var superPos = 0;

        var tBase = def.GetType().BaseType;
        while (tBase is not null)
        {
            if (_typeDefsByType.TryGetValue(tBase, out var super)
                || _discoveredTypeDefsByType.TryGetValue(tBase, out super))
            {
                superset[superPos] = super.Id;
            }

            tBase = tBase.BaseType;
                
            if (++superPos >= MagicNumbers.MaxSubsetPath)
            {
                throw new PipeLoomException($"Initial type hierarchy cannot be deeper then {MagicNumbers.MaxSubsetPath} levels");
            }
        }

        if (superPos > 0)
        {
            def.Superset = superset[..superPos];
        }
    }

    private bool TryGetClosestCommonAncestor(IEnumerable<PlTypeDef> over, [MaybeNullWhen(false)] out PlTypeDef common)
    {
        common = null;
        
        if (over.TryGetNonEnumeratedCount(out var overCount))
        {
            switch (overCount)
            {
                case 0:
                    return false;
                case 1:
                    common = over.Single();
                    return true;
            }
        }

        using var enumerator = over.GetEnumerator();
        if (!enumerator.MoveNext())
            return false;

        var needle = enumerator.Current.Superset;
        var needleLength = needle.Length;
        
        switch (needleLength)
        {
            case <= 0:
                return false;
            case > MagicNumbers.MaxSubsetPath:
                throw new PipeLoomException($"Inconsistent needle length. Must not exceed max subset path ({MagicNumbers.MaxSubsetPath})");
        }

        var alive = (1UL << needleLength) - 1;

        while (enumerator.MoveNext())
        {
            var haystack = enumerator.Current.Superset;
            
            var remaining = alive;

            while (remaining != 0)
            {
                var i = BitOperations.TrailingZeroCount(remaining);
                remaining &= remaining - 1;

                var candidate = needle[i];

                if (Array.BinarySearch(haystack, candidate) < 0)
                {
                    alive &= ~(1UL << i);
                }
            }

            if (alive == 0)
                return false;
        }
        
        Debug.Assert(alive > 0);

        var bits = alive;
        while (bits != 0)
        {
            var i = BitOperations.TrailingZeroCount(bits);
            bits &= bits - 1;

            var candidate = this.GetDef(needle[i]);
            common ??= candidate;

            if (candidate.IsSubsetOf(common))
                common = candidate;
        }

        return common is not null;
    }

    private PlTypeDef GetDef(int typeId)
    {
        if (!_typeDefsById.TryGetValue(typeId, out var res)
            && !_discoveredTypeDefsById.TryGetValue(typeId, out res))
        {
            throw new PipeLoomException($"Type of id '{typeId}' cannot be found");
        }

        return res;
    }
}