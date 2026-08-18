using System;
using System.Linq;
using System.Runtime.CompilerServices;
using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Abstractions;

public enum PlTypeCardinality
{ 
    Unknown = 0, 
    One = 1,
    Many = 2
}

public abstract class PlTypeDef
{
    public IPipeLoomEngine Engine { get; }
    public abstract string Name { get; }
    public abstract PlTypeCardinality Cardinality { get; }
    public abstract bool IsFloating { get; }
    
    public abstract Type NativeType { get; }
    
    public int Id { get; }

    public bool IsOpenGeneric => this is PlGenericType;

    public Variant DefaultValue => _defaultValue ??= this.GetDefaultValue();

    internal int[] Superset
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            
            field = value;
            field.Sort();
            
            this.SupersetMax = field.Length == 0 ? -1 : field.Max();
        }
    } = [];

    internal int SupersetMax { get; private set; }

    private Variant? _defaultValue;

    protected PlTypeDef(IPipeLoomEngine engine)
    {
        this.Engine = engine;
        
        this.Id = engine.NextTypeId();
    }
    
    protected abstract Variant GetDefaultValue();

    protected internal virtual void SetupConverters(scoped in ConverterRegistrator convertible)
    {
        this.SetupMyConverters(convertible.From(this));
    }

    protected virtual void SetupMyConverters(scoped in FromDefConverter fromMyself)
    {
    }

    public virtual bool IsConvertibleTo(PlTypeDef target)
    {
        return this.Engine.Conversions.IsConvertible(this, target);
    }

    public virtual Variant ConvertTo(scoped in Variant value, PlTypeDef target)
    {
        return this.Engine.Conversions.Convert(in value, target);
    }
    
    public virtual string? VariantToStringForDebug(scoped in Variant v)
    {
        return null;
    }

    public virtual bool IsAncestorOf(PlTypeDef def)
    {
        return def.IsSubsetOf(this);
    }

    public virtual bool IsSubsetOf(PlTypeDef def)
    {
        return this.IsSubsetOf(def.Id);
    }

    private bool IsSubsetOf(int typeId)
    {
        return this.Superset.Contains(typeId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong CombineIds(PlTypeDef first, PlTypeDef second)
    {
        return ((ulong)first.Id << 32) | (uint)second.Id;
    }
}

public abstract class PlTypeDef<TNative> : PlTypeDef
{
    public override Type NativeType => typeof(TNative);
    public override bool IsFloating => false;

    protected PlTypeDef(IPipeLoomEngine engine)
        : base(engine)
    {
    }

    protected override Variant GetDefaultValue()
    {
        return Variant.From(default(TNative), this);
    }
}