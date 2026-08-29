using System;
using PipeLoom.Engine.TypeConversions;

namespace PipeLoom.Engine.Abstractions;

public interface IPlConverter
{
    IPipeLoomEngine Engine { get; }
    PlTypeDef SourceType { get; }
    PlTypeDef TargetType { get; }
    
    Variant Convert(IWeaveContext context, scoped in Variant value);
}

public interface IPlTargetConverter<TTarget> : IPlConverter
{
    public delegate TTarget Converter(IWeaveContext context, scoped in Variant v);

    IPlTargetConverter<TTarget> Using(Converter converter);
}

public interface IPlOpaqueConverter : IPlConverter
{
    public delegate Variant Converter(IWeaveContext context, scoped in Variant v);

    IPlOpaqueConverter Using(Converter converter);
}

public interface IPlRefToRefConverter<TSource, TTarget> : IPlConverter
    where TSource : class where TTarget : class
{
    IPlRefToRefConverter<TSource, TTarget> Using(Func<IWeaveContext, TSource, TTarget> converter);
}

public interface IPlRefToValueConverter<TSource, TTarget> : IPlConverter
    where TSource : class where TTarget : struct
{
    IPlRefToValueConverter<TSource, TTarget> Using(Func<IWeaveContext, TSource, TTarget> converter);
}

public interface IPlValueToRefConverter<TSource, TTarget> : IPlConverter
    where TSource : struct where TTarget : class
{
    public delegate TTarget Converter(IWeaveContext context, scoped in TSource source);

    IPlValueToRefConverter<TSource, TTarget> Using(Converter converter);
}

public interface IPlValueToValueConverter<TSource, TTarget> : IPlConverter
    where TSource : struct where TTarget : struct
{
    public delegate TTarget Converter(IWeaveContext context, scoped in TSource source);

    IPlValueToValueConverter<TSource, TTarget> Using(Converter converter);
}

public readonly struct FromRefConverter<T> where T: class
{
    private readonly ConverterRegistrator _registrator;

    public FromRefConverter(in ConverterRegistrator registrator)
    {
        _registrator = registrator;
    }
    
    public IPlRefToRefConverter<T, TTarget> ToRef<TTarget>()
        where TTarget: class
    {
        return _registrator.Add(new PlRefToRefConverter<T, TTarget>(_registrator.Engine));
    }
    
    public IPlRefToValueConverter<T, TTarget> ToValue<TTarget>()
        where TTarget: struct
    {
        return _registrator.Add(new PlRefToValueConverter<T, TTarget>(_registrator.Engine));
    }
}

public readonly struct FromValueConverter<T> where T: struct
{
    private readonly ConverterRegistrator _registrator;

    public FromValueConverter(in ConverterRegistrator registrator)
    {
        _registrator = registrator;
    }
    
    public IPlValueToRefConverter<T, TTarget> ToRef<TTarget>()
        where TTarget: class
    {
        return _registrator.Add(new PlValueToRefConverter<T, TTarget>(_registrator.Engine));
    }
    
    public IPlValueToValueConverter<T, TTarget> ToValue<TTarget>()
        where TTarget: struct
    {
        return _registrator.Add(new PlValueToValueConverter<T, TTarget>(_registrator.Engine));
    }
}

public readonly struct FromDefConverter
{
    private readonly ConverterRegistrator _registrator;
    private readonly PlTypeDef _def;

    public FromDefConverter(in ConverterRegistrator registrator, PlTypeDef def)
    {
        _registrator = registrator;
        _def = def;
    }

    public IPlTargetConverter<TTarget> To<TTarget>()
    {
        return _registrator.Add(new PlTargetConverter<TTarget>(_def, _registrator.Engine));
    }

    public IPlOpaqueConverter To(PlTypeDef targetDef)
    {
        return _registrator.Add(new PlOpaqueConverter(_def, targetDef, _registrator.Engine));
    }
}

public readonly struct ConverterRegistrator
{
    public readonly IPipeLoomEngine Engine;
    
    internal ConverterRegistrator(PipeLoomEngine engine)
    {
        Engine = engine;
    }

    public FromDefConverter From(PlTypeDef def)
    {
        return new FromDefConverter(this, def);
    }

    public FromRefConverter<T> FromRef<T>() where T : class
    {
        return new FromRefConverter<T>(this);
    }
    
    public FromValueConverter<T> FromValue<T>() where T : struct
    {
        return new FromValueConverter<T>(this);
    }

    public FromValueConverter<Variant> FromVariant()
    {
        return this.FromValue<Variant>();
    }
    
    internal TConverter Add<TConverter>(TConverter converter)
        where TConverter: PlConverter
    {
        Engine.Conversions.Add(converter);

        return converter;
    }
}