using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Api.Teams;

/// <summary>
/// Query-string parameters for <c>GET /api/Teams</c>: paging, sorting and the
/// optional team filters. Bound from the query string, mapped to
/// <c>GetTeamsPageQuery</c> in the action.
/// </summary>
public sealed record GetTeamsPageRequest
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = PaginationDefaults.DefaultPageSize;

    public string? Sort { get; init; }

    public string? Name { get; init; }

    public string? City { get; init; }

    public FootballType? Type { get; init; }

    public Division? Division { get; init; }

    public AgeCategory? Category { get; init; }

    public bool? Active { get; init; }
}
