namespace FairPlay.Sports.Domain.Users;

public sealed class User
{
    public Guid Id { get; }
    public string UserName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Team { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; }
    public bool Active { get; private set; }

    private User(
        Guid id,
        string userName,
        string email,
        string passwordHash,
        string team,
        UserRole role,
        DateTime createdAt,
        bool active)
    {
        Id = id;
        UserName = userName;
        Email = email;
        PasswordHash = passwordHash;
        Team = team;
        Role = role;
        CreatedAt = createdAt;
        Active = active;
    }

    public static User Create(
        Guid id,
        string userName,
        string email,
        string passwordHash,
        string team,
        DateTime createdAtUtc,
        UserRole role = UserRole.Member)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(id));
        return new(id, ValidateUserName(userName), ValidateEmail(email), ValidatePasswordHash(passwordHash), ValidateTeam(team), role, createdAtUtc, active: true);
    }

    public void ChangePassword(string newPasswordHash) => PasswordHash = ValidatePasswordHash(newPasswordHash);

    public void MoveToTeam(string team) => Team = ValidateTeam(team);

    public void PromoteToAdmin() => Role = UserRole.Admin;

    public void DemoteToMember() => Role = UserRole.Member;

    public void Deactivate() => Active = false;

    public void Reactivate() => Active = true;

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

    private static string ValidateTeam(string team)
    {
        if (string.IsNullOrWhiteSpace(team))
            throw new ArgumentException("Team is required.", nameof(team));

        return team.Trim();
    }
}
