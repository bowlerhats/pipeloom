using System;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Bundles;
using PipeLoom.Engine.Abstractions.Bundles.ListSources;
using PipeLoom.Engine.Pools;

namespace PipeLoom.Engine.Bundles;

internal sealed class BundleFactory : IBundleFactory, IPoolReturnable
{
    private WeaveContext? _context;

    private WeaveContext Context => _context ?? throw new ArgumentNullException();
    
    public void Bind(WeaveContext context)
    {
        _context = context;
    }

    public void Unbind()
    {
        _context = null;
    }

    public IBundle<T> Create<T>()
    {
        var pool = this.Context.Pools.GetBundlePool<T>();
        var lease = pool.Lease();
        try
        {
            var bundle = lease.Item;
            bundle.Bind(this.Context);
            
            return bundle;
        }
        catch (Exception)
        {
            lease.Forget();
            
            throw;
        }
    }
    
    public LeasedList<T> LeaseList<T>()
    {
        return LeasedList<T>.Lease(this.Context);
    }

    public SingleItemSource<T> SingleItemSource<T>()
    {
        var pool = this.Context.Pools.GetObjectPool<SingleItemSource<T>>(MagicNumbers.SingleItemSourcePoolSize);
        var lease = pool.Lease();

        return lease.Item;
    }

    public ReturnResult OnReturn(IObjectPool _)
    {
        this.Unbind();
        return ReturnResult.Ok();
    }
}