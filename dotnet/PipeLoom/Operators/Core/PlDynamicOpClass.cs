using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Operators.Core;

internal sealed class PlDynamicOpClass : PlOperatorClass
{
    public PlDynamicOpClass(IPipeLoomEngine engine, string name)
        : base(engine, name)
    {
    }
}