using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
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
            _ => new MemCachedPoolSet(),
            MagicNumbers.EnginePoolSetSize);
        
        _conversionMap = new ConversionMap(this);
        
        _typeMap = new TypeMap(this, config);
        
        _conversionMap.Add(_typeMap.TypeDefs);
        
        this.WellKnown = new PlWellknown{ Engine = this };
        
        this.Operators = new OperatorRegistry(this, config);
        
        _typeMap.Compact();
        
        _conversionMap.Add(config.GlobalConverterRegistrations);
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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Variant ConvertValue(scoped in Variant value, PlTypeDef target)
    {
        this.CheckDisposed();
        
        return _conversionMap.Convert(in value, target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryConvert(scoped in Variant value, PlTypeDef target, out Variant converted)
    {
        this.CheckDisposed();
        
        return _conversionMap.TryConvert(in value, target, out converted);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryConvert<TTarget>(scoped in Variant value, out TTarget converted)
    {
        this.CheckDisposed();
        
        return _conversionMap.TryConvert(in value, out converted);
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
        
        throw new NotImplementedException();
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

    public async ValueTask<TOutput> Execute<TOutput>(WeavePlan plan)
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

            var result = await context.Step();

            result = this.ConvertValue(in result, outputType);

            return result.Unpack<TOutput>(reinterpret: true);
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

    
}