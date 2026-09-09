using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Common.Querying;
using MediatR;

namespace FairPlay.Sports.Application.Teams.GetPage;

public sealed class GetTeamsPageHandler(ITeamRepository repository)
    : IRequestHandler<GetTeamsPageQuery, Result<PagedResult<TeamDto>>>
{
    private readonly ITeamRepository _repository = repository;

    public async Task<Result<PagedResult<TeamDto>>> Handle(GetTeamsPageQuery request, CancellationToken cancellationToken)
    {
        var page = await _repository.GetPageAsync(
            request.Filter,
            new TeamSort(request.Sort),
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result<PagedResult<TeamDto>>.Success(page.Map(TeamDto.FromDomain));
    }
}
