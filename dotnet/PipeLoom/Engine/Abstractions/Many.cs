using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions.Bundles;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine.Abstractions;

public static class Many
{
    public static Many<T> Empty<T>()
    {
        return Many<T>.Empty;
    }
    
    public static Many<T> Single<T>(T item)
    {
        return Many<T>.Create(item);
    }

    public static Many<T> Wrap<T>(List<T> source)
    {
        return Many<T>.Create(source);
    }

    public static Many<T> CopyFrom<T>(List<T> source)
    {
        return Many<T>.Create(source.ToList());
    }

    public static Many<T> Create<T>(ReadOnlySpan<T> items, IWeaveContext? context = null)
    {
        return items.Length switch
        {
            0 => Many<T>.Empty,
            1 => Many<T>.Create(items[0]),
            _ => Many<T>.Create(items, context)
        };
    }
}

public enum ManyStoreKind
{
    Empty = 0,
    Single = 1,
    Source = 2,
    List = 3,
    LeasedList = 4
}

public readonly struct Many<T> : IVariantDecomposable<Many<T>>, IForcedStaticalyInitialized
{
    public static readonly Many<T> Empty = default;

    internal static Many<T> Create(ReadOnlySpan<T> items, IWeaveContext? context)
    {
        if (context is null)
            return new Many<T>(items);

        var leased = context.Bundles.LeaseList<T>();
        leased.ReplaceItems(items);

        return new Many<T>(leased);
    }
    
    internal static Many<T> Create(List<T> list)
    {
        return new Many<T>(list);
    }

    internal static Many<T> Create(T item)
    {
        return new Many<T>(item);
    }
    
    private readonly ManyStoreKind _kind;
    private readonly T? _single;
    private readonly IListSource? _source;
    private readonly List<T>? _list;
    private readonly ILeasedList<T>? _leased;
    
    public int Length => this.GetLength();
    public T this[int index] => this.GetItem(index);

    private Many(ManyStoreKind kind)
    {
        _kind = kind;
    }

    private Many(T item)
        : this(ManyStoreKind.Single)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        _single = item;
    }

    private Many(IListSource source)
        : this(ManyStoreKind.Source)
    {
        _source = source;
    }

    [OverloadResolutionPriority(1)]
    private Many(ReadOnlySpan<T> items)
        : this(ManyStoreKind.List)
    {
        _list = new List<T>(items.Length);
        _list.AddRange(items);
    }
    
    private Many(List<T> items)
        : this(ManyStoreKind.List)
    {
        ArgumentNullException.ThrowIfNull(items);
        
        _list = items;
    }

    private Many(ILeasedList<T> leased)
        : this(ManyStoreKind.LeasedList)
    {
        ArgumentNullException.ThrowIfNull(leased);
        
        _leased = leased;
    }
    
    [Pure]
    public ReadOnlySpan<T> AsSpan()
    {
        return _kind switch
        {
            ManyStoreKind.Empty => ReadOnlySpan<T>.Empty,
            ManyStoreKind.Single => new [] { _single! },
            ManyStoreKind.Source => throw new NotSupportedException(),
            ManyStoreKind.List => CollectionsMarshal.AsSpan(_list),
            ManyStoreKind.LeasedList => _leased is IUnsafeSpanProvider<T> provider
                ? provider.UnsafeAsSpan()
                : throw new PipeLoomException("Leased list does not provide spans via IUnsafeSpanProvider<T>"),
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
            ManyStoreKind.Empty => new List<T>(),
            ManyStoreKind.Single => new List<T>(1) { _single! },
            ManyStoreKind.Source => throw new NotSupportedException(),
            ManyStoreKind.List => _list?.ToList(),
            ManyStoreKind.LeasedList => _leased?.ToList(),
            _ => throw UnknownKindError()
        } ?? [];
    }

    [Pure]
    public Many<Variant> ToVariantMany(IWeaveContext context)
    {
        return _kind switch
        {
            ManyStoreKind.Empty => Many.Empty<Variant>(),
            ManyStoreKind.Single => Many.Single(Variant.From(_single, context.Engine)),
            ManyStoreKind.Source => throw new NotSupportedException(),
            ManyStoreKind.List or ManyStoreKind.LeasedList => this.PackVariants(context),
            _ => throw UnknownKindError()
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
        switch (_kind)
        {
            case ManyStoreKind.Empty:
                return Many.Empty<TOther>();
            case ManyStoreKind.Single:
                var cSingle = converterFunc(state, _single!);
                return Many.Single(cSingle);
            case ManyStoreKind.Source:
                throw new NotSupportedException();
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
                // todo: convert to non-boxing if possible
                var local = this;
                Parallel.For(0, itemCount, MagicNumbers.DefaultParallelOptions,
                    i =>
                    {
                        buffer[i] = converterFunc(state, local[i]);
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
            ManyStoreKind.Single => Enumerable.Repeat(_single!, 1),
            ManyStoreKind.List => _list,
            ManyStoreKind.LeasedList => _leased,
            ManyStoreKind.Source => throw new NotSupportedException(),
            _ => throw UnknownKindError()
        } ?? [];
    }

    [Pure]
    public Many<T> Add(T item)
    {
        switch (_kind)
        {
            case ManyStoreKind.Empty:
                return new Many<T>(item);
            case ManyStoreKind.Single:
                return new Many<T>([_single!, item]);
            case ManyStoreKind.Source:
                throw new NotSupportedException();
            case ManyStoreKind.List:
                return this.AddAsList(item);
            case ManyStoreKind.LeasedList:
                if (_leased is null)
                    return this.AddAsList(item);

                var newLeased = _leased.Clone();
                newLeased.Add(item);

                return new Many<T>(newLeased);
            default:
                throw UnknownKindError();
        }
    }

    private Many<T> AddAsList(T item)
    {
        var newCapacity = this.Length + 1;
        
        var listResult = new List<T>(newCapacity);
        
        listResult.AddRange(this.AsEnumerable());

        listResult.Add(item);
                
        return new Many<T>(listResult);
    }

    private int GetLength()
    {
        return _kind switch
        {
            ManyStoreKind.Empty => 0,
            ManyStoreKind.Single => 1,
            ManyStoreKind.Source => 0,
            ManyStoreKind.List => _list?.Count ?? 0,
            ManyStoreKind.LeasedList => _leased?.Count ?? 0,
            _ => throw UnknownKindError()
        };
    }

    private T GetItem(int index)
    {
        return _kind switch
        {
            ManyStoreKind.Empty => throw new IndexOutOfRangeException(),
            ManyStoreKind.Single => index == 0 ? _single! : throw new IndexOutOfRangeException(),
            ManyStoreKind.Source => throw new NotSupportedException(),
            ManyStoreKind.List => _list is not null ? _list[index] : throw new IndexOutOfRangeException(),
            ManyStoreKind.LeasedList => _leased is not null ? _leased[index] : throw new IndexOutOfRangeException(),
            _ => throw UnknownKindError()
        };
    }

    private static InvalidOperationException UnknownKindError()
    {
        return new InvalidOperationException("Unrecognized kind of Many");
    }
    
    #region Decomposable

    static Many()
    {
        VariantDecomposeRegistrar<Many<T>>.EnsureRegistered();
        DoubleDispatch<T>.Register();
    }
    
    public (object? reference, Many<T> bare) DecomposeForVariant()
    {
        object? reference = _kind switch
        {
            ManyStoreKind.Empty => null,
            ManyStoreKind.Single => RuntimeHelpers.IsReferenceOrContainsReferences<T>() ? _single : null,
            ManyStoreKind.Source => _source,
            ManyStoreKind.List => _list,
            ManyStoreKind.LeasedList => _leased,
            _ => throw UnknownKindError()
        };

        var bare = new Many<T>(_kind);
        
        return (reference, bare);
    }

    public static Many<T> ComposeFromPair(object? reference, Many<T> bare)
    {
        if (reference is null)
        {
            return bare._kind switch
            {
                ManyStoreKind.Empty => default,
                ManyStoreKind.Single when !RuntimeHelpers.IsReferenceOrContainsReferences<T>() => bare,
                _ => throw new PipeLoomException("Invalidly deconstructed Many")
            };
        }
        
        return bare._kind switch
        {
            ManyStoreKind.Empty => throw new PipeLoomException("Invalid decomposed Many state. Empty has a reference?!"),
            ManyStoreKind.Single => new Many<T>((T)reference),
            ManyStoreKind.Source => new Many<T>((IListSource)reference),
            ManyStoreKind.List => new Many<T>((List<T>)reference),
            ManyStoreKind.LeasedList => new Many<T>((ILeasedList<T>)reference),
            _ => throw UnknownKindError()
        };
    }
    
    #endregion

    public struct Enumerator : IEnumerator<T>
    {
        private readonly Many<T> _many;
        private List<T>.Enumerator _listEnumerator = default;
        private LeasedListEnumerator<T> _leasedEnumerator = default;
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
                case ManyStoreKind.Single:
                    break;
                case ManyStoreKind.Source:
                    throw new NotSupportedException();
                case ManyStoreKind.List:
                    _listEnumerator = many._list!.GetEnumerator();
                    break;
                case ManyStoreKind.LeasedList:
                    _leasedEnumerator = many._leased!.GetEnumerator();
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
                case ManyStoreKind.List:
                    _listEnumerator.Dispose();
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
                case ManyStoreKind.Single:
                    this.Current = _many._single!;
                    _finished = true;
                    return true;
                case ManyStoreKind.Source:
                    throw new NotSupportedException();
                case ManyStoreKind.List:
                    if (!_listEnumerator.MoveNext())
                    {
                        _finished = true;
                        this.Current = default!;
                        return false;
                    }

                    this.Current = _listEnumerator.Current;
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
                case ManyStoreKind.Single:
                    break;
                case ManyStoreKind.Source:
                    throw new NotSupportedException();
                case ManyStoreKind.List:
                    _listEnumerator.Dispose();
                    _listEnumerator = _many._list!.GetEnumerator();
                    break;
                case ManyStoreKind.LeasedList:
                    _leasedEnumerator.Dispose();
                    _leasedEnumerator = _many._leased!.GetEnumerator();
                    break;
            }
            
            this.Current = default!;
        }
    }
}
