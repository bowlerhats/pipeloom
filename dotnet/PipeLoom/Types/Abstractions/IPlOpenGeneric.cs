using System.Collections.Generic;

namespace PipeLoom.Types.Abstractions;

public interface IPlConstructed
{
    PlGenericType GenericType { get; }
    IReadOnlyList<PlTypeDef> GenericArguments { get; }
}

public interface IPlConstructed<TGeneric> : IPlConstructed
    where TGeneric: PlGenericType
{
    new TGeneric GenericType { get; }
    
    PlGenericType IPlConstructed.GenericType => this.GenericType;
}