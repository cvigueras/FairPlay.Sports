using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.GetById;

public sealed class GetTeamByIdHandler(ITeamRepository repository) : IRequestHandler<GetTeamByIdQuery, Result<TeamDto>>
{
    private readonly ITeamRepository _repository = repository;

    public async Task<Result<TeamDto>> Handle(GetTeamByIdQuery request, CancellationToken cancellationToken)
    {
        var team = await _repository.GetByIdAsync(request.Id, cancellationToken);

        return team is null
            ? Result<TeamDto>.NotFound($"Team '{request.Id}' was not found.")
            : Result<TeamDto>.Success(TeamDto.FromDomain(team));
    }
}
