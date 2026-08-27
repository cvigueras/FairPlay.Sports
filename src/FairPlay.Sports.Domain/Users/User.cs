namespace FairPlay.Sports.Domain.Users;

/// <summary>
/// Aggregate root for an application user. The model is persistence-ignorant:
/// invariants are enforced here, mapping lives in the Infrastructure layer.
/// The password is never held in clear text - only its hash.
/// </summary>
public sealed class User
{
    public Guid Id { get; }
    public string UserName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Team { get; private set; }
    public DateTime CreatedAt { get; }
    public bool Active { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// Full constructor. Also used by EF Core constructor binding when materializing
    /// rows (every parameter name matches a mapped property).
    /// </summary>
    public User(
        Guid id,
        string userName,
        string email,
        string passwordHash,
        string team,
        DateTime createdAt,
        bool active,
        DateTime? lastLoginAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(id));

        Id = id;
        UserName = ValidateUserName(userName);
        Email = ValidateEmail(email);
        PasswordHash = ValidatePasswordHash(passwordHash);
        Team = ValidateTeam(team);
        CreatedAt = createdAt;
        Active = active;
        LastLoginAt = lastLoginAt;
    }

    /// <summary>Registers a brand new, active user. <paramref name="createdAtUtc"/> must be UTC.</summary>
    public static User Register(
        Guid id,
        string userName,
        string email,
        string passwordHash,
        string team,
        DateTime createdAtUtc) =>
        new(id, userName, email, passwordHash, team, createdAtUtc, active: true, lastLoginAt: null);

    /// <summary>Records a successful sign-in. <paramref name="whenUtc"/> must be UTC.</summary>
    public void RecordLogin(DateTime whenUtc) => LastLoginAt = whenUtc;

    public void ChangePassword(string newPasswordHash) => PasswordHash = ValidatePasswordHash(newPasswordHash);

    public void MoveToTeam(string team) => Team = ValidateTeam(team);

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
