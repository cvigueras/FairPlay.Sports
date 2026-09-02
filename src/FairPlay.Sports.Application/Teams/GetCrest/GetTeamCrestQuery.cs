using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.GetCrest;

public sealed record GetTeamCrestQuery(Guid TeamId) : IRequest<Result<TeamCrest>>;
