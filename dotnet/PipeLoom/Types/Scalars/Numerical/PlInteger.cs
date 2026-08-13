using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Types.Scalars;

public class PlInteger : PlWholeNumber<int>
{
    public PlInteger(IPipeLoomEngine engine) : base(engine)
    {
    }

    // protected override void SetupConverters(scoped in FromDefConverter fromMyself)
    // {
    //     base.SetupConverters(in fromMyself);
    //     
    //     fromMyself.To<long>().Using(static (scoped in v) => v.Unpack<int>());
    // }
}