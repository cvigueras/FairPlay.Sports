using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.Activate;

public sealed class ActivateTeamHandler(ITeamRepository repository) : IRequestHandler<ActivateTeamCommand, Result>
{
    private readonly ITeamRepository _repository = repository;

    public async Task<Result> Handle(ActivateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await _repository.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (team is null)
            return Result.NotFound($"Team '{request.Id}' was not found.");

        team.Activate();

        return Result.Success();
    }
}
