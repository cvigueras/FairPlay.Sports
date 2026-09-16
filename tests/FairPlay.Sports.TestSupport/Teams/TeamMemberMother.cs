using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.JoinTeam;
using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.TestSupport.Teams;

public static class TeamMemberMother
{
    public static readonly Guid TeamId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid UserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public const TeamMemberRole Role = TeamMemberRole.Player;
    public const string DisplayName = "Carlos V.";

    public static JoinTeamCommand JoinCommand(Guid? teamId = null, Guid? userId = null, TeamMemberRole? role = null) =>
        new(teamId ?? TeamId, userId ?? UserId, role ?? Role, DisplayName);

    public static TeamMember DomainTeamMember(
        Guid? id = null,
        Guid? teamId = null,
        Guid? userId = null,
        TeamMemberRole? role = null,
        string? displayName = null,
        DateTime? createdAtUtc = null) =>
        TeamMember.Create(
            id ?? Guid.NewGuid(),
            teamId ?? TeamId,
            userId ?? UserId,
            role ?? Role,
            displayName ?? DisplayName,
            createdAtUtc ?? DateTime.UtcNow);

    public static TeamMemberDto Dto(Guid? id = null, Guid? teamId = null, Guid? userId = null) =>
        new(id ?? Guid.NewGuid(), teamId ?? TeamId, userId ?? UserId, Role, DisplayName, DateTime.UtcNow);

    public static string AlreadyMember => "This user already belongs to this team.";

    public static string RoleAlreadyTaken(TeamMemberRole role) => $"This team already has a {role}.";
}
