namespace PipeLoom.Engine.Pools;

public readonly record struct ReturnResult(
    bool ShouldDrop = false
    )
{
    public static ReturnResult Ok()
    {
        return new ReturnResult();
    }

    public static ReturnResult Drop()
    {
        return new ReturnResult(ShouldDrop: true);
    }
}

public interface IPoolReturnable
{
    ReturnResult OnReturn(IObjectPool pool);
}