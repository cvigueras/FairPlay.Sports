namespace FairPlay.Sports.Domain.Auth;

/// <summary>
/// A single issued refresh token. Only its SHA-256 hash is ever stored, so a database
/// leak does not hand out usable tokens. Tokens are single-use: <see cref="Revoke"/> is
/// called on rotation and the replacement is linked through
/// <see cref="ReplacedByTokenId"/>, which lets the login flow spot a replayed token and
/// revoke the whole family.
/// </summary>
public sealed class RefreshToken
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public string TokenHash { get; }
    public DateTime CreatedAtUtc { get; }
    public DateTime ExpiresAtUtc { get; }
    public DateTime? RevokedAtUtc { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static RefreshToken Issue(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Refresh token id cannot be empty.", nameof(id));
        if (userId == Guid.Empty)
            throw new ArgumentException("Refresh token must belong to a user.", nameof(userId));
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Refresh token hash is required.", nameof(tokenHash));
        if (expiresAtUtc <= createdAtUtc)
            throw new ArgumentException("Refresh token must expire after it is created.", nameof(expiresAtUtc));

        return new(id, userId, tokenHash.Trim(), createdAtUtc, expiresAtUtc);
    }

    public bool IsActive(DateTime nowUtc) => RevokedAtUtc is null && nowUtc < ExpiresAtUtc;

    /// <summary>Marks the token spent. Idempotent: a second call is a no-op.</summary>
    public void Revoke(DateTime nowUtc, Guid? replacedByTokenId = null)
    {
        if (RevokedAtUtc is not null)
            return;

        RevokedAtUtc = nowUtc;
        ReplacedByTokenId = replacedByTokenId;
    }
}
