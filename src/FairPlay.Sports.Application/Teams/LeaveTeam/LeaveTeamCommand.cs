using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.LeaveTeam;

public sealed record LeaveTeamCommand(Guid TeamId, Guid UserId) : IRequest<Result>;
