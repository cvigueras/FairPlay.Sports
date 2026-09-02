using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.GetById;

public sealed record GetTeamByIdQuery(Guid Id) : IRequest<Result<TeamDto>>;
