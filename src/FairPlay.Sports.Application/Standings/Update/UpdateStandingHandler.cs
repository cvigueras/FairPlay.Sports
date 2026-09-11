using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using MediatR;

namespace FairPlay.Sports.Application.Standings.Update;

public sealed class UpdateStandingHandler(IStandingRepository standings, ITeamRepository teams)
    : IRequestHandler<UpdateStandingCommand, Result<StandingDto>>
{
    private readonly IStandingRepository _standings = standings;
    private readonly ITeamRepository _teams = teams;

    public async Task<Result<StandingDto>> Handle(UpdateStandingCommand request, CancellationToken cancellationToken)
    {
        var standing = await _standings.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (standing is null)
            return Result<StandingDto>.NotFound($"Standing '{request.Id}' was not found.");

        var team = await _teams.GetByIdAsync(standing.TeamId, cancellationToken);
        if (team is null)
            return Result<StandingDto>.NotFound($"Team '{standing.TeamId}' was not found.");

        standing.UpdateStats(
            request.Points,
            request.Played,
            request.Won,
            request.Drawn,
            request.Lost,
            request.GoalsFor,
            request.GoalsAgainst);

        return Result<StandingDto>.Success(StandingDto.FromDomain(standing, team));
    }
}
