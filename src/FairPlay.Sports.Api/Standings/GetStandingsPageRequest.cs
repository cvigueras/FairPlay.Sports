using FairPlay.Sports.Application.Common.Querying;

namespace FairPlay.Sports.Api.Standings;

public sealed record GetStandingsPageRequest
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = PaginationDefaults.DefaultPageSize;

    public string? Sort { get; init; }

    public Guid? TeamId { get; init; }
}
