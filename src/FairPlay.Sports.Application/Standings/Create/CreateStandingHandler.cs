using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Domain.Standings;
using MediatR;

namespace FairPlay.Sports.Application.Standings.Create;

public sealed class CreateStandingHandler(IStandingRepository standings, ITeamRepository teams, IClock clock)
    : IRequestHandler<CreateStandingCommand, Result<StandingDto>>
{
    private readonly IStandingRepository _standings = standings;
    private readonly ITeamRepository _teams = teams;
    private readonly IClock _clock = clock;

    public async Task<Result<StandingDto>> Handle(CreateStandingCommand request, CancellationToken cancellationToken)
    {
        var team = await _teams.GetByIdAsync(request.TeamId, cancellationToken);
        if (team is null)
            return Result<StandingDto>.NotFound($"Team '{request.TeamId}' was not found.");

        if (await _standings.ExistsByTeamIdAsync(request.TeamId, cancellationToken))
            return Result<StandingDto>.Failure($"Team '{request.TeamId}' already has a standing.");

        var standing = Standing.Create(
            Guid.NewGuid(),
            request.TeamId,
            request.Points,
            request.Played,
            request.Won,
            request.Drawn,
            request.Lost,
            request.GoalsFor,
            request.GoalsAgainst,
            _clock.UtcNow);

        await _standings.AddAsync(standing, cancellationToken);

        return Result<StandingDto>.Success(StandingDto.FromDomain(standing, team));
    }
}
