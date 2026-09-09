namespace FairPlay.Sports.Application.Common.Querying;

/// <summary>
/// Shape shared by every paged read. A query carrying this contract is picked up
/// by <see cref="PagedQueryValidator{TQuery}"/> for the page/size bounds and can be
/// executed generically against any <c>IQueryable</c> via the repository.
/// </summary>
public interface IPagedQuery
{
    int Page { get; }
    int PageSize { get; }

    /// <summary>
    /// Optional sort expression, comma-separated, <c>-</c> prefix for descending
    /// (for example <c>"name"</c>, <c>"-createdAt"</c>, <c>"city,-createdAt"</c>).
    /// Each slice whitelists the fields it accepts.
    /// </summary>
    string? Sort { get; }
}
