using PipeLoom.Engine.TypeConversions;

namespace PipeLoom.Engine.Abstractions;

public interface IPlConversionMap
{
    VariantPacker<T>? FindCustomVariantPacker<T>();
    VariantUnpacker<T>? FindCustomVariantUnpacker<T>();

    bool IsConvertible(PlTypeDef from, PlTypeDef to);
    Variant Convert(scoped in Variant value, PlTypeDef target);

    internal void Add(PlTypeDef def);
    internal void Add<TConverter>(TConverter converter)
        where TConverter : PlConverter;
}