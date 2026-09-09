namespace FairPlay.Sports.Application.Common.Querying;

/// <summary>
/// Applies an ordering to an <c>IQueryable</c> of <typeparamref name="T"/>. Each
/// aggregate defines its own implementation, whitelisting the sortable fields and
/// always ending on a stable key so paging is deterministic.
/// </summary>
public interface IQuerySort<T>
{
    IQueryable<T> Apply(IQueryable<T> source);
}
