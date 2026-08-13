using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine.TypeConversions;

internal sealed class PlValueToValueConverter<TSource, TTarget> : PlConverter, IPlValueToValueConverter<TSource, TTarget>
    where TSource : struct where TTarget : struct
{
    private IPlValueToValueConverter<TSource, TTarget>.Converter? _converter;

    public IPlValueToValueConverter<TSource, TTarget>.Converter ConverterFunc =>
        _converter ?? throw new PipeLoomException("Missing converter function");
    
    public PlValueToValueConverter(IPipeLoomEngine engine)
        : base(engine.TypeOf<TSource>(), engine.TypeOf<TTarget>(), engine)
    {
    }

    public IPlValueToValueConverter<TSource, TTarget> Using(IPlValueToValueConverter<TSource, TTarget>.Converter converter)
    {
        _converter = converter;

        return this;
    }

    public override Variant Convert(scoped in Variant value)
    {
        if (!value.TryUnpack<TSource>(out var unpacked))
            throw InvalidConversion();
        
        var converted = this.ConverterFunc(in unpacked);
        
        // TODO: assess that converted is in fact targettype
        
        return Variant.From(converted, this.TargetType);
    }
}