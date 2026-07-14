using PipeLoom.Engine.Abstractions.Registration;

namespace PipeLoom.Engine;

internal sealed class OperatorRegistrator : PlOperatorRegistrator
{
    public OperatorRegistrator(PlOperatorClass operatorClass)
        : base(operatorClass)
    {
    }
}