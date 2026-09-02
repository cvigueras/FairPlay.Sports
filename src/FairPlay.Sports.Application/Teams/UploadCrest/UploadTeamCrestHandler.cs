using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.UploadCrest;

public sealed class UploadTeamCrestHandler(ITeamRepository repository) : IRequestHandler<UploadTeamCrestCommand, Result>
{
    private readonly ITeamRepository _repository = repository;

    public async Task<Result> Handle(UploadTeamCrestCommand request, CancellationToken cancellationToken)
    {
        var team = await _repository.GetByIdForUpdateAsync(request.TeamId, cancellationToken);
        if (team is null)
            return Result.NotFound($"Team '{request.TeamId}' was not found.");

        team.SetCrest(request.Content, request.ContentType);

        return Result.Success();
    }
}
