namespace FairPlay.Sports.Domain.Users;

public sealed class User
{
    public const int MaxPhotoBytes = 2 * 1024 * 1024;

    private static readonly string[] AllowedPhotoContentTypes =
        ["image/png", "image/jpeg", "image/webp", "image/svg+xml"];

    public Guid Id { get; }
    public string UserName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public Guid? TeamId { get; private set; }
    public UserRole Role { get; private set; }

    public byte[]? Photo { get; private set; }
    public string? PhotoContentType { get; private set; }

    public DateTime CreatedAt { get; }
    public bool Active { get; private set; } = false;

    public bool HasPhoto => Photo is { Length: > 0 };

    private User(
        Guid id,
        string userName,
        string email,
        string passwordHash,
        Guid? teamId,
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
        Guid? teamId,
        DateTime createdAtUtc,
        UserRole role = UserRole.Member)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(id));
        return new(id, ValidateUserName(userName), ValidateEmail(email), ValidatePasswordHash(passwordHash), ValidateOptionalTeamId(teamId), role, createdAtUtc);
    }

    public void MoveToTeam(Guid teamId) => TeamId = ValidateTeamId(teamId);

    public void Activate() => Active = true;

    public void SetPhoto(byte[] image, string contentType)
    {
        if (image is null || image.Length == 0)
            throw new ArgumentException("Profile photo cannot be empty.", nameof(image));

        if (image.Length > MaxPhotoBytes)
            throw new ArgumentException($"Profile photo cannot exceed {MaxPhotoBytes} bytes.", nameof(image));

        Photo = image;
        PhotoContentType = ValidatePhotoContentType(contentType);
    }

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

        var normalized = email.Trim().ToLowerInvariant();
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

    private static Guid? ValidateOptionalTeamId(Guid? teamId) =>
        teamId is null ? null : ValidateTeamId(teamId.Value);

    private static string ValidatePhotoContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Profile photo content type is required.", nameof(contentType));

        var normalized = contentType.Trim().ToLowerInvariant();
        if (!AllowedPhotoContentTypes.Contains(normalized))
            throw new ArgumentException(
                $"Profile photo content type '{contentType}' is not an accepted image type.", nameof(contentType));

        return normalized;
    }
}
