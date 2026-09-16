namespace FairPlay.Sports.Domain.Teams;

public sealed class TeamMember
{
    public const int MaxDisplayNameLength = 100;

    public Guid Id { get; }
    public Guid TeamId { get; }
    public Guid UserId { get; }
    public TeamMemberRole Role { get; }
    public string DisplayName { get; }
    public DateTime CreatedAt { get; }

    private TeamMember(
        Guid id,
        Guid teamId,
        Guid userId,
        TeamMemberRole role,
        string displayName,
        DateTime createdAt)
    {
        Id = id;
        TeamId = teamId;
        UserId = userId;
        Role = role;
        DisplayName = displayName;
        CreatedAt = createdAt;
    }

    public static TeamMember Create(
        Guid id,
        Guid teamId,
        Guid userId,
        TeamMemberRole role,
        string displayName,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Team member id cannot be empty.", nameof(id));

        return new(
            id,
            ValidateTeamId(teamId),
            ValidateUserId(userId),
            ValidateRole(role),
            ValidateDisplayName(displayName),
            createdAtUtc);
    }

    private static Guid ValidateTeamId(Guid teamId)
    {
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id is required.", nameof(teamId));

        return teamId;
    }

    private static Guid ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id is required.", nameof(userId));

        return userId;
    }

    private static TeamMemberRole ValidateRole(TeamMemberRole role)
    {
        if (!Enum.IsDefined(role))
            throw new ArgumentException("A valid team role is required.", nameof(role));

        return role;
    }

    private static string ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.", nameof(displayName));

        var trimmed = displayName.Trim();
        if (trimmed.Length > MaxDisplayNameLength)
            throw new ArgumentException(
                $"Display name cannot exceed {MaxDisplayNameLength} characters.", nameof(displayName));

        return trimmed;
    }
}
