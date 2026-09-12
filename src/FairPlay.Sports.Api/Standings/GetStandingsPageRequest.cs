using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Api.Standings;

public sealed record GetStandingsPageRequest
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = PaginationDefaults.DefaultPageSize;

    public string? Sort { get; init; }

    public Guid? TeamId { get; init; }

    public FootballType? Type { get; init; }

    public Division? Division { get; init; }

    public AgeCategory? Category { get; init; }
}
