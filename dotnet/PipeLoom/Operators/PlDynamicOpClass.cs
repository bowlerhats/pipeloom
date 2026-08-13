using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Operators;

internal sealed class PlDynamicOpClass : PlOperatorClass
{
    public PlDynamicOpClass(IPipeLoomEngine engine, string name)
        : base(engine, name)
    {
    }
}