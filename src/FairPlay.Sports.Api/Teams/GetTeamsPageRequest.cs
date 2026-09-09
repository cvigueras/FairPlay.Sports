using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Api.Teams;

public sealed record GetTeamsPageRequest
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = PaginationDefaults.DefaultPageSize;

    public string? Sort { get; init; }

    public string? Name { get; init; }

    public string? City { get; init; }

    public string? Coach { get; init; }

    public FootballType? Type { get; init; }

    public Division? Division { get; init; }

    public AgeCategory? Category { get; init; }

    public bool? Active { get; init; }
}
