using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Api.Teams;

public sealed record JoinTeamRequest(Guid UserId, TeamMemberRole Role, string DisplayName);
