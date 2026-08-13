using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine.TypeConversions;

internal sealed class PlTargetConverter<TTarget> : PlConverter, IPlTargetConverter<TTarget>
{
    private IPlTargetConverter<TTarget>.Converter? _converter;
    
    internal IPlTargetConverter<TTarget>.Converter ConverterFunc =>
        _converter ?? throw new PipeLoomException("Missing converter function");
    
    public PlTargetConverter(PlTypeDef sourceType, IPipeLoomEngine engine)
        : base(sourceType, engine.TypeOf<TTarget>(), engine)
    {
    }

    public IPlTargetConverter<TTarget> Using(IPlTargetConverter<TTarget>.Converter converter)
    {
        _converter = converter;

        return this;
    }

    public override Variant Convert(scoped in Variant value)
    {
        var converted = this.ConverterFunc(in value);

        return Variant.From(converted, this.TargetType);
    }
}