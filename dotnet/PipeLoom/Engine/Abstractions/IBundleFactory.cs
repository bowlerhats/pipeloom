using PipeLoom.Engine.Abstractions.Bundles;
using PipeLoom.Engine.Abstractions.Bundles.ListSources;
using PipeLoom.Engine.Bundles;

namespace PipeLoom.Engine.Abstractions;

public interface IBundleFactory
{
    IBundle<T> Create<T>();

    LeasedList<T> LeaseList<T>();

    internal SingleItemSource<T> SingleItemSource<T>();
}