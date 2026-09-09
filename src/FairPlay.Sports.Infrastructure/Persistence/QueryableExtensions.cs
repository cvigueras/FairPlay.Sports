using FairPlay.Sports.Application.Common.Querying;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Persistence;

/// <summary>
/// Turns an already-filtered, already-ordered <c>IQueryable</c> into one
/// <see cref="PagedResult{T}"/>: one <c>COUNT</c> for the total, then
/// <c>OFFSET</c>/<c>LIMIT</c> for the page. Repositories compose the filter and
/// sort, then call this last.
/// </summary>
internal static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await source.CountAsync(cancellationToken);

        var items = await source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, page, pageSize, totalCount);
    }
}
