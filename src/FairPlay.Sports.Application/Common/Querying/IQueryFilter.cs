namespace FairPlay.Sports.Application.Common.Querying;

/// <summary>
/// A strongly-typed, composable filter over an <c>IQueryable</c> of <typeparamref name="T"/>.
/// Each aggregate defines its own implementation (the fields it exposes for
/// filtering); the repository just calls <see cref="Apply"/> before paging, so the
/// whole thing still translates to a single SQL query.
/// </summary>
public interface IQueryFilter<T>
{
    IQueryable<T> Apply(IQueryable<T> source);
}
