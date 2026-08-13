using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;

namespace PipeLoom.Engine.TypeConversions;

internal abstract class PlConverter : IPlConverter
{
    public IPipeLoomEngine Engine { get; }
    
    public PlTypeDef SourceType { get; }
    public PlTypeDef TargetType { get; }

    public ulong TypeId => PlTypeDef.CombineIds(this.SourceType, this.TargetType);

    protected PlConverter(
        PlTypeDef sourceType,
        PlTypeDef targetType,
        IPipeLoomEngine engine)
    {
        this.SourceType = sourceType;
        this.TargetType = targetType;
        this.Engine = engine;
    }

    public abstract Variant Convert(scoped in Variant value);

    protected static PipeLoomException InvalidConversion()
    {
        return new PipeLoomException("Invalid conversion");
    }
}