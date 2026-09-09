using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Common.Querying;
using MediatR;

namespace FairPlay.Sports.Application.Teams.GetPage;

public sealed record GetTeamsPageQuery(
    int Page,
    int PageSize,
    string? Sort,
    TeamFilter Filter) : IRequest<Result<PagedResult<TeamDto>>>, IPagedQuery;
