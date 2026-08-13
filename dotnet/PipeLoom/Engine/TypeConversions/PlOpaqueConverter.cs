using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine.TypeConversions;

internal sealed class PlOpaqueConverter : PlConverter, IPlOpaqueConverter
{
    private IPlOpaqueConverter.Converter? _converter;
    
    internal IPlOpaqueConverter.Converter ConverterFunc =>
        _converter ?? throw new PipeLoomException("Missing converter function");
    
    public PlOpaqueConverter(PlTypeDef sourceType, PlTypeDef targetType, IPipeLoomEngine engine)
        : base(sourceType, targetType, engine)
    {
    }
    
    public IPlOpaqueConverter Using(IPlOpaqueConverter.Converter converter)
    {
        _converter = converter;

        return this;
    }

    public override Variant Convert(scoped in Variant value)
    {
        return this.ConverterFunc(in value);
    }
}