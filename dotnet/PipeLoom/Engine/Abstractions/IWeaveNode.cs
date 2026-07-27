namespace PipeLoom.Engine.Abstractions;

public interface IWeaveNode
{
    WeavePlan Plan { get; }
    Variant ImplicitValue { get; }
}