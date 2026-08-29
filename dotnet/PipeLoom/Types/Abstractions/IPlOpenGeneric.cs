using System.Collections.Generic;

namespace PipeLoom.Types.Abstractions;

public interface IPlConstructed
{
    PlTypeDef SelfType { get; }
    PlGenericType GenericType { get; }
    IReadOnlyList<PlTypeDef> GenericArguments { get; }
}

public interface IPlConstructed<out TGeneric> : IPlConstructed
    where TGeneric: PlGenericType
{
    new TGeneric GenericType { get; }
    
    PlGenericType IPlConstructed.GenericType => this.GenericType;
}