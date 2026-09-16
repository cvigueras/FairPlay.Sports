using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Teams;

public sealed record TeamMemberDto(
    Guid Id,
    Guid TeamId,
    Guid UserId,
    TeamMemberRole Role,
    string DisplayName,
    DateTime CreatedAt)
{
    public static TeamMemberDto FromDomain(TeamMember member) =>
        new(member.Id, member.TeamId, member.UserId, member.Role, member.DisplayName, member.CreatedAt);
}
