using FairPlay.Sports.Application.Common.Querying;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Persistence;

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
