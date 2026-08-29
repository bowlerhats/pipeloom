using System;
using System.Diagnostics.CodeAnalysis;

namespace PipeLoom.Engine.Pools;

public interface IObjectPool : IDisposable
{
    bool IsDisposed { get; }
    
    /// <summary>
    /// Releases a tracked lease and returns the item to the pool.
    /// </summary>
    /// <remarks>
    /// Prefer disposing the <see cref="Lease{T}"/> directly over calling this method.
    /// This method is intended as a callback for the <see cref="Lease{T}.Release"/> itself.
    /// </remarks>
    /// <param name="ticket">The ticket issued when the lease was created.</param>
    /// <exception cref="InvalidOperationException">Thrown when the ticket is not found in the lease tracking.</exception>
    void Release(long ticket);
    
    /// <summary>
    /// Removes a tracked lease without returning the item to the pool.
    /// </summary>
    /// <remarks>
    /// After forgetting, the item must be manually returned via <see cref="Return"/> if it is no longer needed. <br/>
    /// Prefer <see cref="Lease{T}.Forget"/> over calling this method directly.
    /// </remarks>
    /// <param name="ticket">The ticket issued when the lease was created.</param>
    /// <exception cref="InvalidOperationException">Thrown when the ticket is not found in the lease tracking.</exception>
    void Forget(long ticket);
    
    /// <summary>
    /// Clears the pool and optionally forcefully disposes all active leases.
    /// </summary>
    /// <remarks>
    /// When <paramref name="alsoLeases"/> is <see langword="true"/>, any outstanding
    /// <see cref="Lease{T}"/> will become invalid and throw on subsequent access or disposal.
    /// </remarks>
    /// <param name="alsoLeases">If <see langword="true"/>, also disposes and untracks all active leases.</param>
    void Clear(bool alsoLeases = false);

    /// <summary>
    /// Releases all active leases and returns the leased items to the pool
    /// </summary>
    /// <remarks>It does a best effort to release everything. Race conditions might leave leases active</remarks>
    void ReleaseAll();

    /// <summary>
    /// Prefills the pool to provided percentage
    /// </summary>
    void Warmup(uint percentage = 50);

    /// <summary>
    /// Checks if lease is tracked and active
    /// </summary>
    /// <param name="ticket">Ticket number of the lease</param>
    /// <returns>True, if the lease is active</returns>
    bool IsLeaseActive(long ticket);
}

public interface IObjectPool<T> : IObjectPool
    where T: class
{
    /// <summary>
    /// Rents an item from the pool and returns it as a tracked lease.
    /// </summary>
    /// <remarks>
    /// The item is automatically returned to the pool when the lease is disposed. <br/>
    /// Use <see cref="LeaseUntracked"/> to opt out of tracking for performance-sensitive paths.
    /// </remarks>
    /// <returns>A tracked <see cref="Lease{T}"/> containing the rented item.</returns>
    Lease<T> Lease();
    
    /// <summary>
    /// Rents an item from the pool and returns it as an untracked lease.
    /// </summary>
    /// <remarks>
    /// The item is automatically returned to the pool when the lease is disposed,
    /// but the lease is not tracked by the pool. <br/>
    /// Use and prefer <see cref="Lease"/> when not on hot path.
    /// </remarks>
    /// <returns>An untracked <see cref="Lease{T}"/> containing the rented item.</returns>
    Lease<T> LeaseUntracked();
    
    /// <summary>
    /// Tries to rent an item from the pool and return it as a tracked lease.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="null"/> when the pool is exhausted. <br/>
    /// Use <see cref="TryLeaseUntracked"/> to opt out of tracking for performance-sensitive paths.
    /// </remarks>
    /// <returns>A tracked <see cref="Lease{T}"/> containing the rented item, or <see langword="null"/> if the pool is exhausted.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a double return or pool corruption is detected.</exception>
    Lease<T>? TryLease();
    
    /// <summary>
    /// Tries to rent an item from the pool and return it as an untracked lease.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="null"/> when the pool is exhausted. <br/>
    /// Use and prefer <see cref="TryLease"/> when not on hot path.
    /// </remarks>
    /// <returns>An untracked <see cref="Lease{T}"/> containing the rented item, or <see langword="null"/> if the pool is exhausted.</returns>
    Lease<T>? TryLeaseUntracked();
    
    /// <summary>
    /// Retrieves the item associated with the given ticket from the pool's lease tracking.
    /// </summary>
    /// <remarks>
    /// Throws if the ticket is not found, which indicates the lease has already been
    /// released, forgotten, or the ticket is otherwise invalid.
    /// </remarks>
    /// <param name="ticket">The ticket issued when the lease was created.</param>
    /// <returns>The item associated with the ticket.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the ticket is not found in the lease tracking.</exception>
    T GetLeasedItem(long ticket);
    
    /// <summary>
    /// Returns an untracked item to the pool.
    /// </summary>
    /// <remarks>
    /// Returning an item held by tracked leases will be ignored. <br/>
    /// An item is untracked if obtained by <see cref="Rent"/> or <see cref="TryRentNoCreate"/>,
    /// or a lease was <see cref="Forget"/>.
    /// </remarks>
    /// <param name="item">An untracked item to return to the pool.</param>
    void Return(T item);
    
    /// <summary>
    /// Rents an item from the pool which must be manually returned via <see cref="Return"/>.
    /// </summary>
    /// <returns>Item pulled from the pool or freshly created if pool is exhausted.</returns>
    T Rent();
    
    /// <summary>
    /// Tries to rent an item from the pool without creating one when the pool is exhausted.
    /// </summary>
    /// <param name="rented">Item pulled from the pool.</param>
    /// <returns><see langword="true"/> if an item was available; <see langword="false"/> if the pool is exhausted.</returns>
    bool TryRentNoCreate([MaybeNullWhen(false)] out T rented);
}