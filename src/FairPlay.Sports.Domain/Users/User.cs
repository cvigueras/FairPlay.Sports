namespace FairPlay.Sports.Domain.Users;

public sealed class User
{
    public Guid Id { get; }
    public string UserName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public Guid TeamId { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; }
    public bool Active { get; private set; } = false;

    private User(
        Guid id,
        string userName,
        string email,
        string passwordHash,
        Guid teamId,
        UserRole role,
        DateTime createdAt)
    {
        Id = id;
        UserName = userName;
        Email = email;
        PasswordHash = passwordHash;
        TeamId = teamId;
        Role = role;
        CreatedAt = createdAt;
    }

    public static User Create(
        Guid id,
        string userName,
        string email,
        string passwordHash,
        Guid teamId,
        DateTime createdAtUtc,
        UserRole role = UserRole.Member)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(id));
        return new(id, ValidateUserName(userName), ValidateEmail(email), ValidatePasswordHash(passwordHash), ValidateTeamId(teamId), role, createdAtUtc);
    }

    public void ChangePassword(string newPasswordHash) => PasswordHash = ValidatePasswordHash(newPasswordHash);

    public void MoveToTeam(Guid teamId) => TeamId = ValidateTeamId(teamId);

    public void PromoteToAdmin() => Role = UserRole.Admin;

    public void DemoteToMember() => Role = UserRole.Member;

    public void Deactivate() => Active = false;

    public void Activate() => Active = true;

    private static string ValidateUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name is required.", nameof(userName));

        return userName.Trim();
    }

    private static string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        var normalized = email.Trim();
        if (!normalized.Contains('@', StringComparison.Ordinal))
            throw new ArgumentException("Email is not a valid address.", nameof(email));

        return normalized;
    }

    private static string ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return passwordHash;
    }

    private static Guid ValidateTeamId(Guid teamId)
    {
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id is required.", nameof(teamId));

        return teamId;
    }
}
