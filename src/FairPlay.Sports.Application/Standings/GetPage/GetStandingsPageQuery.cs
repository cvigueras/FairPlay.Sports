using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Common.Querying;
using MediatR;

namespace FairPlay.Sports.Application.Standings.GetPage;

public sealed record GetStandingsPageQuery(
    int Page,
    int PageSize,
    string? Sort,
    StandingFilter Filter) : IRequest<Result<PagedResult<StandingDto>>>, IPagedQuery;
