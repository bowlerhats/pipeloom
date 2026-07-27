using System;

namespace PipeLoom.Engine.Abstractions;

public interface IPlConversionMap
{
    Converter<T, Variant> ToVariant<T>();
    Converter<Variant, T> FromVariant<T>();
}