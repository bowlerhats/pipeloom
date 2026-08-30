using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions.Bundles;
using PipeLoom.Engine.Abstractions.Bundles.ListSources;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine.Abstractions;

public static class Many
{
    public static Many<T> Empty<T>()
    {
        return Many<T>.Empty;
    }
    
    public static Many<T> Single<T>(T item, IWeaveContext context)
    {
        var source = context.Bundles.SingleItemSource<T>();
        source.Item = item;

        return new Many<T>(source);
    }

    public static Many<T> Wrap<T>(T[] source)
    {
        return new Many<T>(source);
    }

    public static Many<T> Create<T>(ReadOnlySpan<T> items, IWeaveContext context)
    {
        return items.Length switch
        {
            0 => Many<T>.Empty,
            1 => Single(items[0], context),
            _ => Leased(items, context)
        };
    }

    public static Many<T> Concat<T>(IWeaveContext context, ReadOnlySpan<T> items, params ReadOnlySpan<T> others)
    {
        var total = items.Length + others.Length;
        switch (total)
        {
            case 0 : return Empty<T>();
            case 1 : return items.Length > 0 ? Single(items[0], context) : Single(others[0], context);
        }

        var pool = context.Pools.GetArrayPool<T>();
        var buffer = pool.Rent(total);
        try
        {
            if (items.Length > 0)
                items.CopyTo(buffer);
            
            if (others.Length > 0)
                others.CopyTo(buffer.AsSpan(items.Length));

            return Leased(buffer.AsSpan(0, total), context);
        }
        finally
        {
            pool.Return(buffer, true);
        }
    }

    private static Many<T> Leased<T>(ReadOnlySpan<T> items, IWeaveContext context)
    {
        var leased = context.Bundles.LeaseList<T>();
        leased.ReplaceItems(items);
        
        return new Many<T>(leased);
    }
}

public enum ManyStoreKind
{
    Empty = 0,
    Array = 1,
    Source = 2,
    LeasedList = 3
}

[StructLayout(LayoutKind.Sequential, Size = 16)]
public readonly struct Many<T> : IVariantDecomposable<Many<T>>, IForcedStaticalyInitialized
{
    public static readonly Many<T> Empty;
    
    private readonly ManyStoreKind _kind;
    private readonly object? _store;
    
    public int Length => this.GetLength();
    public T this[int index] => this.GetItem(index);
    
    private T[] AsArray => (T[])(_store ?? throw MissingStore());
    private IListSource<T> AsSource => (IListSource<T>)(_store ?? throw MissingStore());
    private ILeasedList<T> AsLeased => (ILeasedList<T>)(_store ?? throw MissingStore());
    private IUnsafeSpanProvider<T> AsUnsafe => _store as IUnsafeSpanProvider<T> ?? throw UnsupportedSpan();

    private Many(ManyStoreKind kind)
    {
        _kind = kind;
    }
    
    internal Many(T[] array)
        : this(ManyStoreKind.Array)
    {
        ArgumentNullException.ThrowIfNull(array);
        
        _store = array;
    }

    internal Many(IListSource<T> source)
        : this(ManyStoreKind.Source)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        _store = source;
    }

    internal Many(ILeasedList<T> leased)
        : this(ManyStoreKind.LeasedList)
    {
        ArgumentNullException.ThrowIfNull(leased);
        
        _store = leased;
    }

    [Pure]
    public ReadOnlySpan<T> AsSpan()
    {
        return _kind switch
        {
            ManyStoreKind.Empty => ReadOnlySpan<T>.Empty,
            ManyStoreKind.Array => this.AsArray.AsSpan(),
            ManyStoreKind.Source or ManyStoreKind.LeasedList
                => this.AsUnsafe.UnsafeAsSpan(),
            _ => throw UnknownKindError()
        };
    }
    
    [Pure]
    public ReadOnlyMemory<T> AsMemory()
    {
        return _kind switch
        {
            ManyStoreKind.Empty => ReadOnlyMemory<T>.Empty,
            ManyStoreKind.Array => this.AsArray.AsMemory(),
            ManyStoreKind.Source or ManyStoreKind.LeasedList
                => this.AsUnsafe.UnsafeAsMemory(),
            _ => throw UnknownKindError()
        };
    }
    
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }
    
    [Pure]
    public List<T> ToList()
    {
        return _kind switch
        {
            ManyStoreKind.Empty => [],
            ManyStoreKind.Array => this.AsArray.ToList(),
            ManyStoreKind.Source => this.AsSource.ToList(),
            ManyStoreKind.LeasedList => this.AsLeased.ToList(),
            _ => throw UnknownKindError()
        };
    }

    [Pure]
    public Many<Variant> ToVariantMany(IWeaveContext context)
    {
        return this.Length switch
        {
            0 => Many.Empty<Variant>(),
            1 => Many.Single(Variant.From(this.GetItem(0), context.Engine), context),
            _ => this.PackVariants(context)
        };
    }

    [Pure]
    public Many<TOther> ConvertTo<TOther>(IWeaveContext context, Func<T, TOther> converterFunc)
    {
        return this.ConvertTo(context, converterFunc, static (func, item) => func(item));
    }
    
    [Pure]
    public Many<TOther> ConvertTo<TOther, TState>(IWeaveContext context, TState state, Func<TState, T, TOther> converterFunc)
    {
        switch (this.Length)
        {
            case 0 : return Many.Empty<TOther>();
            case 1 : return Many.Single(converterFunc(state, this.GetItem(0)), context);
        }

        var itemCount = this.Length;
        var pool = context.Pools.GetArrayPool<TOther>();
        var buffer = pool.Rent(itemCount);
        try
        {
            if (itemCount < MagicNumbers.ParallelLinearCutoff || MagicNumbers.ParallelForcedLinear)
            {
                var span = this.AsSpan();
                for (var i = 0; i < itemCount; i++)
                {
                    buffer[i] = converterFunc(state, span[i]);
                }
            }
            else
            {
                var memory = this.AsMemory();
                Parallel.For(0, itemCount, MagicNumbers.DefaultParallelOptions,
                    i =>
                    {
                        buffer[i] = converterFunc(state, memory.Span[i]);
                    });
            }

            return Many.Create(buffer.AsSpan(0, itemCount), context);
        }
        finally
        {
            pool.Return(buffer, true);
        }
    }
    
    [Pure]
    private Many<Variant> PackVariants(IWeaveContext context)
    {
        var itemType = context.Engine.TypeOf<T>();
        
        return this.ConvertTo(context, itemType, static (hint, d) => Variant.From(d, hint));
    }

    [Pure]
    public IEnumerable<T> AsEnumerable()
    {
        return _kind switch
        {
            ManyStoreKind.Empty => [],
            ManyStoreKind.Array => this.AsArray,
            ManyStoreKind.Source => this.AsSource.AsEnumerable(),
            ManyStoreKind.LeasedList => this.AsLeased,
            _ => throw UnknownKindError()
        };
    }

    [Pure]
    public Many<T> Add(T item)
    {
        switch (_kind)
        {
            case ManyStoreKind.Empty:
                return new Many<T>([item]);
            case ManyStoreKind.Array:
                return new Many<T>([..this.AsArray, item]);
            case ManyStoreKind.Source:
                if (this.AsSource.TryAddImmutable(item, out var newSource))
                {
                    return new Many<T>(newSource);
                }

                if (this.AsSource.Context is not null)
                {
                    return Many.Concat(this.AsSource.Context, this.AsSpan(), item);
                }

                return Many.Wrap([.. this.AsSpan(), item]);
            case ManyStoreKind.LeasedList:
                
                var newLeased = this.AsLeased.Clone();
                newLeased.Add(item);

                return new Many<T>(newLeased);
            default:
                throw UnknownKindError();
        }
    }

    private int GetLength()
    {
        if (_store is null)
            return 0;
        
        return _kind switch
        {
            ManyStoreKind.Empty => 0,
            ManyStoreKind.Array => this.AsArray.Length,
            ManyStoreKind.Source => this.AsSource.Count,
            ManyStoreKind.LeasedList => this.AsLeased.Count,
            _ => throw UnknownKindError()
        };
    }

    private T GetItem(int index)
    {
        return _kind switch
        {
            ManyStoreKind.Empty => throw new IndexOutOfRangeException(),
            ManyStoreKind.Array => this.AsArray[index],
            ManyStoreKind.Source => this.AsSource.GetItem(index),
            ManyStoreKind.LeasedList => this.AsLeased[index],
            _ => throw UnknownKindError()
        };
    }

    private static InvalidOperationException UnknownKindError()
    {
        return new InvalidOperationException("Unrecognized kind of Many");
    }

    private static InvalidOperationException MissingStore()
    {
        return new InvalidOperationException("Missing underlying item store for Many");
    }

    private static PipeLoomException UnsupportedSpan()
    {
        return new PipeLoomException("Underlying type could not provide span or memory");
    }
    
    #region Decomposable

    static Many()
    {
        VariantDecomposeRegistrar<Many<T>>.EnsureRegistered();
        DoubleDispatch<T>.Register();
    }
    
    public (object? reference, Many<T> bare) DecomposeForVariant()
    {
        var reference = _store;

        var bare = new Many<T>(_kind);
        
        return (reference, bare);
    }

    public static Many<T> ComposeFromPair(object? reference, Many<T> bare)
    {
        if (reference is null)
        {
            return bare._kind == ManyStoreKind.Empty
                ? default
                : throw new PipeLoomException("Non-empty Many needs a reference to reconstruct");
        }
        
        return bare._kind switch
        {
            ManyStoreKind.Empty => throw new PipeLoomException("Invalid decomposed Many state. Empty has a reference?!"),
            ManyStoreKind.Array => new Many<T>((T[])reference),
            ManyStoreKind.Source => new Many<T>((IListSource<T>)reference),
            ManyStoreKind.LeasedList => new Many<T>((ILeasedList<T>)reference),
            _ => throw UnknownKindError()
        };
    }
    
    #endregion

    public struct Enumerator : IEnumerator<T>
    {
        private readonly Many<T> _many;
        private ArraySegment<T>.Enumerator _arrayEnumerator = default;
        private LeasedListEnumerator<T> _leasedEnumerator = default;
        private ListSourceEnumerator<T> _sourceEnumerator = default!;
        private bool _finished;
        
        public T Current { get; private set; }

        object? IEnumerator.Current => this.Current;

        internal Enumerator(Many<T> many)
        {
            _many = many;
            
            _finished = false;

            switch (many._kind)
            {
                case ManyStoreKind.Empty:
                    _finished = true;
                    break;
                case ManyStoreKind.Array:
                    _arrayEnumerator = new ArraySegment<T>(many.AsArray).GetEnumerator();
                    break;
                case ManyStoreKind.Source:
                    _sourceEnumerator = many.AsSource.GetEnumerator();
                    break;
                case ManyStoreKind.LeasedList:
                    _leasedEnumerator = many.AsLeased.GetEnumerator();
                    break;
                default:
                    throw UnknownKindError();
            }

            this.Current = default!;
        }
        
        public void Dispose()
        {
            switch (_many._kind)
            {
                case ManyStoreKind.Array:
                    _arrayEnumerator.Dispose();
                    break;
                case ManyStoreKind.Source:
                    _sourceEnumerator.Dispose();
                    break;
                case ManyStoreKind.LeasedList:
                    _leasedEnumerator.Dispose();
                    break;
            }
        }

        public bool MoveNext()
        {
            if (_finished)
            {
                this.Current = default!;
                return false;
            }

            switch (_many._kind)
            {
                case ManyStoreKind.Empty:
                    throw new PipeLoomException("Attempted enumeration of empty Many");
                case ManyStoreKind.Array:
                    if (!_arrayEnumerator.MoveNext())
                    {
                        _finished = true;
                        this.Current = default!;
                        return false;
                    }

                    this.Current = _arrayEnumerator.Current;
                    break;
                case ManyStoreKind.Source:
                    if (!_sourceEnumerator.MoveNext())
                    {
                        _finished = true;
                        this.Current = default!;
                        return false;
                    }

                    this.Current = _sourceEnumerator.Current;
                    break;
                case ManyStoreKind.LeasedList:
                    if (!_leasedEnumerator.MoveNext())
                    {
                        _finished = true;
                        this.Current = default!;
                        return false;
                    }

                    this.Current = _leasedEnumerator.Current;
                    break;
            }
            
            return true;
        }

        public void Reset()
        {
            _finished = false;
            
            switch (_many._kind)
            {
                case ManyStoreKind.Empty:
                    _finished = true;
                    break;
                case ManyStoreKind.Source:
                    _sourceEnumerator.Dispose();
                    _sourceEnumerator = _many.AsSource.GetEnumerator();
                    break;
                case ManyStoreKind.Array:
                    _arrayEnumerator.Dispose();
                    _arrayEnumerator = new ArraySegment<T>(_many.AsArray).GetEnumerator();
                    break;
                case ManyStoreKind.LeasedList:
                    _leasedEnumerator.Dispose();
                    _leasedEnumerator = _many.AsLeased.GetEnumerator();
                    break;
            }
            
            this.Current = default!;
        }
    }
}
