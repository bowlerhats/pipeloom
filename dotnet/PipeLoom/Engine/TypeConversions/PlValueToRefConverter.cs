using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine.TypeConversions;

internal sealed class PlValueToRefConverter<TSource, TTarget> : PlConverter, IPlValueToRefConverter<TSource, TTarget>
    where TSource : struct where TTarget : class
{
    private IPlValueToRefConverter<TSource, TTarget>.Converter? _converter;
    
    public IPlValueToRefConverter<TSource, TTarget>.Converter ConverterFunc =>
        _converter ?? throw new PipeLoomException("Missing converter function");
    
    public PlValueToRefConverter(IPipeLoomEngine engine)
        : base(engine.TypeOf<TSource>(), engine.TypeOf<TTarget>(), engine)
    {
    }
    
    public IPlValueToRefConverter<TSource, TTarget> Using(IPlValueToRefConverter<TSource, TTarget>.Converter converter)
    {
        _converter = converter;
        
        return this;
    }

    public override Variant Convert(IWeaveContext context, scoped in Variant value)
    {
        if (!value.TryUnpack<TSource>(out var unpacked))
            throw InvalidConversion();
        
        var converted = this.ConverterFunc(context, in unpacked);
        
        // TODO: assess that converted is in fact targettype
        
        return Variant.From(converted, this.TargetType);
    }
}