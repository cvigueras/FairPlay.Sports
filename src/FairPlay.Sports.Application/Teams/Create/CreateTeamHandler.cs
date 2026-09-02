using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Domain.Teams;
using MediatR;

namespace FairPlay.Sports.Application.Teams.Create;

public sealed class CreateTeamHandler(ITeamRepository repository) : IRequestHandler<CreateTeamCommand, Result<TeamDto>>
{
    private readonly ITeamRepository _repository = repository;

    public async Task<Result<TeamDto>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.ExistsByNameAsync(request.Name, cancellationToken))
            return Result<TeamDto>.Failure($"Team '{request.Name}' already exists.");

        var team = Team.Create(
            Guid.NewGuid(),
            request.Name,
            request.Coach,
            request.City,
            request.Type,
            request.Division,
            request.Category,
            DateTime.UtcNow);

        await _repository.AddAsync(team, cancellationToken);

        return Result<TeamDto>.Success(TeamDto.FromDomain(team));
    }
}
