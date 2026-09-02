using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.GetCrest;

public sealed class GetTeamCrestHandler(ITeamRepository repository) : IRequestHandler<GetTeamCrestQuery, Result<TeamCrest>>
{
    private readonly ITeamRepository _repository = repository;

    public async Task<Result<TeamCrest>> Handle(GetTeamCrestQuery request, CancellationToken cancellationToken)
    {
        var crest = await _repository.GetCrestAsync(request.TeamId, cancellationToken);

        return crest is null
            ? Result<TeamCrest>.NotFound($"Team '{request.TeamId}' has no crest.")
            : Result<TeamCrest>.Success(crest);
    }
}
