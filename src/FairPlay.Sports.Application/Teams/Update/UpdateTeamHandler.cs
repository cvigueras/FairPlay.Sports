using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using MediatR;

namespace FairPlay.Sports.Application.Teams.Update;

public sealed class UpdateTeamHandler(ITeamRepository repository) : IRequestHandler<UpdateTeamCommand, Result<TeamDto>>
{
    private readonly ITeamRepository _repository = repository;

    public async Task<Result<TeamDto>> Handle(UpdateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await _repository.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (team is null)
            return Result<TeamDto>.NotFound($"Team '{request.Id}' was not found.");

        var name = request.Name.Trim();
        if (await _repository.ExistsByNameAsync(name, request.Type, request.Division, request.Category, team.Id, cancellationToken))
        {
            return Result<TeamDto>.Failure($"A team named '{name}' already exists in this modality, division and category.");
        }

        team.Update(
            request.Name,
            request.Coach,
            request.City,
            request.ToClassification(),
            request.ToProfile());

        return Result<TeamDto>.Success(TeamDto.FromDomain(team));
    }
}
