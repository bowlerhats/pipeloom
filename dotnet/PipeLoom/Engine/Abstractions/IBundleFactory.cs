using PipeLoom.Engine.Abstractions.Bundles;
using PipeLoom.Engine.Bundles;
using PipeLoom.Engine.Pools;

namespace PipeLoom.Engine.Abstractions;

public interface IBundleFactory
{
    IBundle<T> Create<T>();

    LeasedList<T> LeaseList<T>();
}