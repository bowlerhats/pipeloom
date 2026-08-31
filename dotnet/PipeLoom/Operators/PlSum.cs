using System.Numerics.Tensors;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;

namespace PipeLoom.Operators;

public class PlSum : PlOperatorClass
{
    public PlSum(IPipeLoomEngine engine)
        : base(engine, "sum")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        registrator.AsUnary<long>().Reducer(Sum);
        
        // todo: bugged registration order, overshadowed by long reducer
        // registrator.AsUnary<int>().Reducer(Sum);
        
        //registrator.AsUnary<IBundle<long>>().Function(BundleSum);
    }

    private static long Sum(Many<long> items)
    {
        return items.Length != 0 ? TensorPrimitives.Sum(items.AsSpan()) : 0;
    }
    
    // private static int Sum(Many<int> items)
    // {
    //     return items.Length != 0 ? TensorPrimitives.Sum(items.AsSpan()) : 0;
    // }
    
    // private static long BundleSum(IBundle<long> bundle)
    // {
    //     var res = 0L;
    //     foreach (var partition in bundle.Partitions)
    //     {
    //         res += TensorPrimitives.Sum(partition.Leaf.AsSpan());
    //     }
    //
    //     return res;
    // }
}

public class PlSumExtended : PlSum
{
    public PlSumExtended(IPipeLoomEngine engine)
        : base(engine)
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        base.RegisterHandlers(registrator);

        registrator.AsUnary<ulong>().Reducer(Sum);
    }
    
    private static ulong Sum(Many<ulong> items)
    {
        return items.Length != 0 ? TensorPrimitives.Sum(items.AsSpan()) : 0;
    }
}