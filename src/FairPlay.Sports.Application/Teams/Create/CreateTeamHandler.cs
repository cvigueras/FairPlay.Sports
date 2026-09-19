using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Standings;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Domain.Standings;
using FairPlay.Sports.Domain.Teams;
using MediatR;

namespace FairPlay.Sports.Application.Teams.Create;

public sealed class CreateTeamHandler(ITeamRepository repository, IStandingRepository standings, IClock clock)
    : IRequestHandler<CreateTeamCommand, Result<TeamDto>>
{
    private readonly ITeamRepository _repository = repository;
    private readonly IStandingRepository _standings = standings;
    private readonly IClock _clock = clock;

    public async Task<Result<TeamDto>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (await _repository.ExistsByNameAsync(name, request.Type, request.Division, request.Category, cancellationToken: cancellationToken))
            return Result<TeamDto>.Failure($"A team named '{name}' already exists in this modality, division and category.");

        var team = Team.Create(
            Guid.NewGuid(),
            request.Name,
            request.Coach,
            request.City,
            request.ToClassification(),
            _clock.UtcNow,
            request.ToProfile());

        await _repository.AddAsync(team, cancellationToken);

        var standing = Standing.Create(Guid.NewGuid(), team.Id, 0, 0, 0, 0, 0, 0, 0, _clock.UtcNow);
        await _standings.AddAsync(standing, cancellationToken);

        return Result<TeamDto>.Success(TeamDto.FromDomain(team));
    }
}
