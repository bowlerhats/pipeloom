using System.Collections.Generic;

namespace PipeLoom.Engine.Abstractions;

public interface IWeaveNode
{
    WeavePlan Plan { get; }
    Variant ImplicitValue { get; }
    
    IReadOnlyList<IWeaveNode> Children { get; }
}