using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.GetAll;

public sealed class GetAllTeamsHandler(ITeamRepository repository) : IRequestHandler<GetAllTeamsQuery, Result<IReadOnlyList<TeamDto>>>
{
    private readonly ITeamRepository _repository = repository;

    public async Task<Result<IReadOnlyList<TeamDto>>> Handle(GetAllTeamsQuery request, CancellationToken cancellationToken)
    {
        var result = (await _repository.GetAllAsync(cancellationToken)).Select(TeamDto.FromDomain).ToList();
        return Result<IReadOnlyList<TeamDto>>.Success(result);
    }
}
