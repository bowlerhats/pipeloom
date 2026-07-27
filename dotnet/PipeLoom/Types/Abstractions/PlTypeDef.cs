using System;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;

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
    public abstract Type NativeType { get; }

    public bool IsOpenGeneric => this is PlGenericType;

    public Variant DefaultValue => _defaultValue ??= this.GetDefaultValue();

    private Variant? _defaultValue;

    protected PlTypeDef(IPipeLoomEngine engine)
    {
        this.Engine = engine;
    }
    
    protected abstract Variant GetDefaultValue();

    public virtual bool IsAssignableTo(PlTypeDef target)
    {
        return this.Equals(target);
    }

    public virtual Variant AssignTo(Variant value, PlTypeDef target)
    {
        return value.UncheckedCastAs(target.NativeType, target);
    }

    public virtual bool IsConvertibleTo(PlTypeDef target)
    {
        return false;
    }

    public virtual Variant ConvertTo(Variant value, PlTypeDef target)
    {
        throw new NotImplementedException();
    }
}

public abstract class PlTypeDef<TNative> : PlTypeDef
{
    public override Type NativeType => typeof(TNative);

    protected PlTypeDef(IPipeLoomEngine engine)
        : base(engine)
    {
    }

    protected override Variant GetDefaultValue()
    {
        return Variant.From(default(TNative), this);
    }

    public virtual Variant ToVariant(TNative native)
    {
        return Variant.From(native, this);
    }
}