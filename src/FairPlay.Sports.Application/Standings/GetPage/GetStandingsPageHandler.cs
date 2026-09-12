using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Application.Teams;
using MediatR;

namespace FairPlay.Sports.Application.Standings.GetPage;

public sealed class GetStandingsPageHandler(IStandingRepository standings, ITeamRepository teams)
    : IRequestHandler<GetStandingsPageQuery, Result<PagedResult<StandingDto>>>
{
    private readonly IStandingRepository _standings = standings;
    private readonly ITeamRepository _teams = teams;

    public async Task<Result<PagedResult<StandingDto>>> Handle(GetStandingsPageQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        // Standing carries no navigation to Team, so a classification filter is resolved to
        // concrete team ids here before the paged query runs.
        if (filter.Type is not null || filter.Division is not null || filter.Category is not null)
        {
            var matchingTeamIds = await _teams.GetIdsByClassificationAsync(
                filter.Type, filter.Division, filter.Category, cancellationToken);
            filter = filter with { TeamIds = matchingTeamIds };
        }

        var page = await _standings.GetPageAsync(
            filter,
            new StandingSort(request.Sort),
            request.Page,
            request.PageSize,
            cancellationToken);

        var teamIds = page.Items.Select(standing => standing.TeamId).Distinct();
        var teamsById = (await _teams.GetByIdsAsync(teamIds, cancellationToken)).ToDictionary(team => team.Id);

        return Result<PagedResult<StandingDto>>.Success(
            page.Map(standing => StandingDto.FromDomain(standing, teamsById[standing.TeamId])));
    }
}
