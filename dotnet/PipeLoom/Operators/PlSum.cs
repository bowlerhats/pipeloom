using System.Linq;
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
        registrator.AsUnary<int>().Reducer(Sum);
    }

    private static int Sum(Many<int> items)
    {
        return items.Length != 0 ? items.AsList().Sum() : 0;
    }
}