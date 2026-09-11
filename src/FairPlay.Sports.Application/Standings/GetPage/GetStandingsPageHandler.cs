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
        var page = await _standings.GetPageAsync(
            request.Filter,
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
