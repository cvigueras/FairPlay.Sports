using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using MediatR;

namespace FairPlay.Sports.Application.Standings.GetById;

public sealed class GetStandingByIdHandler(IStandingRepository standings, ITeamRepository teams)
    : IRequestHandler<GetStandingByIdQuery, Result<StandingDto>>
{
    private readonly IStandingRepository _standings = standings;
    private readonly ITeamRepository _teams = teams;

    public async Task<Result<StandingDto>> Handle(GetStandingByIdQuery request, CancellationToken cancellationToken)
    {
        var standing = await _standings.GetByIdAsync(request.Id, cancellationToken);
        if (standing is null)
            return Result<StandingDto>.NotFound($"Standing '{request.Id}' was not found.");

        var team = await _teams.GetByIdAsync(standing.TeamId, cancellationToken);
        if (team is null)
            return Result<StandingDto>.NotFound($"Team '{standing.TeamId}' was not found.");

        return Result<StandingDto>.Success(StandingDto.FromDomain(standing, team));
    }
}
