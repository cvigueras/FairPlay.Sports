using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Domain.Teams;
using MediatR;

namespace FairPlay.Sports.Application.Teams.JoinTeam;

public sealed record JoinTeamCommand(Guid TeamId, Guid UserId, TeamMemberRole Role, string DisplayName)
    : IRequest<Result<TeamMemberDto>>;
