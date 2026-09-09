namespace FairPlay.Sports.Application.Common.Querying;

/// <summary>
/// One page of results plus the metadata a client needs to page through the rest.
/// <see cref="Map{TOut}"/> keeps that metadata while a handler projects the page
/// (domain entities) to its DTOs.
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => Page > 1;

    public bool HasNext => Page < TotalPages;

    public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector) =>
        new([.. Items.Select(selector)], Page, PageSize, TotalCount);
}
