using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Adapters;
using PipeLoom.Engine.Abstractions.Bundles;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Engine.Pools;
using PipeLoom.Engine.TypeConversions;

namespace PipeLoom.Engine;

public class PipeLoomEngine : IPipeLoomEngine
{
    public IPlConversionMap Conversions => _conversionMap;
    
    public IPlWellknown WellKnown { get; }
    
    private OperatorRegistry Operators { get; }
    
    internal IObjectPool<MemCachedPoolSet> PoolSets { get; }

    internal TypeMap TypeMap => _typeMap;

    TypeMap IPipeLoomEngine.TypeMap => _typeMap;

    // This lock serializes engine-global type, operator and other metadata changes
    // It has to block execution for example if a Compact() is requested
    private readonly ReaderWriterLockSlim _engineLock = new(LockRecursionPolicy.NoRecursion);
    
    private bool _disposed;
    private int _nextTypeId;
    
    private TypeMap _typeMap;
    private ConversionMap _conversionMap;
    
    internal PipeLoomEngine(IEngineConfig config)
    {
        this.PoolSets = new ObjectPool<MemCachedPoolSet>(
            _ => new MemCachedPoolSet(this),
            MagicNumbers.EnginePoolSetSize);
        
        this.PoolSets.Warmup(100);
        
        _conversionMap = new ConversionMap(this);
        
        _typeMap = new TypeMap(this, config);
        
        _conversionMap.Add(_typeMap.TypeDefs);
        
        this.WellKnown = new PlWellknown{ Engine = this };
        
        this.Operators = new OperatorRegistry(this, config);
        
        _typeMap.Compact();
        
        _conversionMap.Add(config.GlobalConverterRegistrations);

        this.ForceRunTypeInitializers();
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;
        
        this.PoolSets.Dispose();
            
        _engineLock.Dispose();
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true))
            return;
        
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Compact()
    {
        this.CheckDisposed();
        
        if (!_engineLock.TryEnterWriteLock(MagicNumbers.EngineWriteLockTimeoutGraceMs))
            return;
        try
        {
            _typeMap.Compact();
        }
        finally
        {
            _engineLock.ExitWriteLock();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PlTypeDef TypeOf<T>()
    {
        this.CheckDisposed();
        
        return _typeMap.TypeOf<T>();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PlGenericType? FindGeneric(Type nativeOpenGenericType)
    {
        this.CheckDisposed();

        return _typeMap.FindGeneric(nativeOpenGenericType);
    }

    public Variant ToVariant<T>(in T value)
    {
        this.CheckDisposed();

        if (typeof(T) == typeof(Variant))
        {
            return Variant.VerbatimCopyUnsafe(in value);
        }

        var packer = _conversionMap.FindCustomVariantPacker<T>();
        if (packer is not null)
        {
            return packer(in value);
        }

        var type = this.TypeOf<T>();
        return Variant.From(value, type);
    }

    public T FromVariant<T>(in Variant value)
    {
        this.CheckDisposed();
        
        var unpacker = _conversionMap.FindCustomVariantUnpacker<T>();
        
        return unpacker is not null ? unpacker(value) : value.Unpack<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsConvertible(PlTypeDef from, PlTypeDef to)
    {
        this.CheckDisposed();
        
        return _conversionMap.IsConvertible(from, to);
    }
    
    public T GetType<T>()
        where T : PlTypeDef
    {
        this.CheckDisposed();
        
        return _typeMap.Find<T>() ?? throw new PipeLoomException($"Type missing: '{typeof(T).Name}'");
    }

    public PlTypeDef CommonBaseOf(IEnumerable<PlTypeDef> types)
    {
        this.CheckDisposed();

        if (!this.TypeMap.TryGetClosestCommonAncestor(types, out var common))
        {
            return this.WellKnown.Variant;
        }

        return common;
    }

    public PlOperatorArity? GuessArity(IEnumerable<PlTypeDef> args)
    {
        if (args.TryGetNonEnumeratedCount(out var arity))
        {
            return PlOperatorArityExtensions.Infer(arity);
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PlOperatorClass GetOperatorClass(string operatorName)
    {
        this.CheckDisposed();
        
        return this.Operators.GetOperatorClass(operatorName);
    }

    public int NextTypeId()
    {
        this.CheckDisposed();
        
        return Interlocked.Increment(ref _nextTypeId);
    }

    public ValueTask<TOutput> Execute<TOutput>(WeavePlan plan)
    {
        return this.Execute<Ignored, TOutput>(plan, default);
    }

    public async ValueTask<TOutput> Execute<TInput, TOutput>(WeavePlan plan, TInput input)
    {
        this.CheckDisposed();
        
        if (!_engineLock.TryEnterReadLock(MagicNumbers.EngineReadLockTimeoutGraceMs))
            throw new PipeLoomException("Execution starter lock timed out.");
        try
        {
            var outputType = this.TypeOf<TOutput>();

            if (!plan.IsFused)
            {
                await plan.Fuse<TOutput>();
            }

            if (!this.IsConvertible(plan.OutputType, outputType))
            {
                throw new PipeLoomException(
                    $"Plan is fused to type '{plan.OutputType.Name}' which is not convertible to requested type '{outputType.Name}'");
            }

            using var context = new WeaveContext(this, plan);
            
            Variant result;
            
            if (typeof(TInput) == typeof(Ignored))
            {
                result = await context.Step(null);
            }
            else if (plan.RootNode.CarryType is null)
            {
                result = await context.Step(null);
            } else
            {
                var carryType = plan.RootNode.CarryType;
                
                var inputType = this.TypeOf<TInput>();
                var vInput = Variant.From(input, inputType);

                if (!this.Conversions.TryConvert(context, in vInput, carryType, out var converted))
                    throw new PipeLoomException($"Provided input of type '{inputType}' could not be converted to carry type of '{carryType}'");

                result = await context.Step(converted);
            }

            result = this.Conversions.Convert(context, in result, outputType);

            return result.Unpack<TOutput>();
        }
        finally
        {
            _engineLock.ExitReadLock();
        }
    }

    public ValueTask<IBundle<Variant>> Execute(WeavePlan plan)
    {
        this.CheckDisposed();
        
        return this.Execute<IBundle<Variant>>(plan);
    }

    public void Touch<T>()
    {
        IPipeLoomEngine.Discover<T>();
        
        DoubleDispatch<T>.Register();
    }
    
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Defs landed in TypeDefs are not trimmed")]
    [UnconditionalSuppressMessage("Trimming", "IL2059", Justification = "Types which have TypeInitializers guaranteed to have static ctor")]
    private void ForceRunTypeInitializers()
    {
        foreach (var def in _typeMap.TypeDefs)
        {
            if (def.IsFloating || def.IsOpenGeneric || def.NativeType == null!)
                continue;

            if (!def.NativeType.IsAssignableTo(typeof(IForcedStaticalyInitialized)))
                continue;
            
#pragma warning disable IL2075
#pragma warning disable IL2059
            if (def.NativeType.TypeInitializer is not null)
            {
                // No AOT concern, because this is just a defensive check,
                // If the type was trimmed there is no way to show up in TypeDefs.
                // If the static ctor was trimmed the 'TypeInitializer' is null
                RuntimeHelpers.RunClassConstructor(def.NativeType.TypeHandle);
            }
#pragma warning restore IL2059
#pragma warning restore IL2075
        }
    }
    
    private struct Ignored{}
}