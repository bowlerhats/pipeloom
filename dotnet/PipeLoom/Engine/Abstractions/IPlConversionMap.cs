using PipeLoom.Engine.TypeConversions;

namespace PipeLoom.Engine.Abstractions;

public interface IPlConversionMap
{
    VariantPacker<T>? FindCustomVariantPacker<T>();
    VariantUnpacker<T>? FindCustomVariantUnpacker<T>();

    bool IsConvertible(PlTypeDef from, PlTypeDef to);
    Variant Convert(IWeaveContext context, scoped in Variant value, PlTypeDef target);
    TTarget Convert<TSource, TTarget>(IWeaveContext context, TSource value);
    TTarget Convert<TTarget>(IWeaveContext context, scoped in Variant value);
    
    bool TryConvert(IWeaveContext context, scoped in Variant value, PlTypeDef target, out Variant converted);
    bool TryConvert<TTarget>(IWeaveContext context, scoped in Variant value, out TTarget converted);
    bool TryConvert<TSource, TTarget>(IWeaveContext context, TSource value, out TTarget converted);

    IPlConverter? FindConverter(PlTypeDef source, PlTypeDef target);

    internal void Add(PlTypeDef def);
    internal void Add<TConverter>(TConverter converter)
        where TConverter : PlConverter;
}