using System;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine.TypeConversions;

internal sealed class PlRefToValueConverter<TSource, TTarget> : PlConverter, IPlRefToValueConverter<TSource, TTarget>
    where TSource : class where TTarget : struct
{
    private Func<TSource, TTarget>? _converter;

    internal Func<TSource, TTarget> ConverterFunc =>
        _converter ?? throw new PipeLoomException("Missing converter function");
    
    public PlRefToValueConverter(IPipeLoomEngine engine)
        : base(engine.TypeOf<TSource>(), engine.TypeOf<TTarget>(), engine)
    {
    }

    public IPlRefToValueConverter<TSource, TTarget> Using(Func<TSource, TTarget> converter)
    {
        _converter = converter;

        return this;
    }
    
    public override Variant Convert(scoped in Variant value)
    {
        if (!value.TryUnpack<TSource>(out var unpacked))
            throw InvalidConversion();
        
        var converted = this.ConverterFunc(unpacked);
        
        // TODO: assess that converted is in fact targettype
        
        return Variant.From(converted, this.TargetType);
    }
}