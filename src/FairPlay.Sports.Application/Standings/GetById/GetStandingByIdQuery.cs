using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Standings.GetById;

public sealed record GetStandingByIdQuery(Guid Id) : IRequest<Result<StandingDto>>;
