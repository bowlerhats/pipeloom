using System.Collections.Generic;

namespace PipeLoom.Engine.Abstractions;

public interface IWeaveNode
{
    string OperatorName { get; }
    bool IsArgument { get; }
    
    WeavePlan Plan { get; }
    Variant ImplicitValue { get; }
    
    IReadOnlyList<IWeaveNode> Children { get; }
}