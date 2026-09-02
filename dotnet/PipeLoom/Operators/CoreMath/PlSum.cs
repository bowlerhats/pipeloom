using System.Numerics.Tensors;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;

namespace PipeLoom.Operators.CoreMath;

public class PlSum : PlOperatorClass
{
    public PlSum(IPipeLoomEngine engine)
        : base(engine, "sum")
    {
    }

    public override void RegisterHandlers(PlOperatorRegistrator registrator)
    {
        registrator.AsUnary<long>().Reducer(Sum);
        registrator.AsUnary<int>().Reducer(Sum);
    }

    private static long Sum(Many<long> items)
    {
        return items.Length != 0 ? TensorPrimitives.Sum(items.AsSpan()) : 0;
    }
    
    private static int Sum(Many<int> items)
    {
        return items.Length != 0 ? TensorPrimitives.Sum(items.AsSpan()) : 0;
    }
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