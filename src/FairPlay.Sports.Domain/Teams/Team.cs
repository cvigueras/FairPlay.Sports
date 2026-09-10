namespace FairPlay.Sports.Domain.Teams;

public sealed class Team
{
    public const int MaxCrestBytes = 2 * 1024 * 1024;

    public const int MaxShortNameLength = 20;
    public const int MaxContactPhoneLength = 30;
    public const int MaxContactEmailLength = 256;
    public const int MaxWebsiteLength = 2048;
    public const int MinFoundedYear = 1850;

    private static readonly string[] AllowedCrestContentTypes =
        ["image/png", "image/jpeg", "image/webp", "image/svg+xml"];

    public Guid Id { get; }
    public string Name { get; private set; }
    public string Coach { get; private set; }
    public string City { get; private set; }

    public TeamClassification Classification { get; private set; } = null!;

    public string? ShortName { get; private set; }
    public int? FoundedYear { get; private set; }
    public Venue? HomeVenue { get; private set; }
    public KitColors? Colors { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? Website { get; private set; }

    public byte[]? Crest { get; private set; }
    public string? CrestContentType { get; private set; }

    public DateTime CreatedAt { get; private init; }

    public bool Active { get; private set; } = false;

    public bool HasCrest => Crest is { Length: > 0 };

    private Team(
        Guid id,
        string name,
        string coach,
        string city)
    {
        Id = id;
        Name = name;
        Coach = coach;
        City = city;
    }

    public static Team Create(
        Guid id,
        string name,
        string coach,
        string city,
        TeamClassification classification,
        DateTime createdAtUtc,
        TeamProfile? profile = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(id));

        ArgumentNullException.ThrowIfNull(classification);

        var team = new Team(id, ValidateName(name), ValidateCoach(coach), ValidateCity(city))
        {
            Classification = classification,
            CreatedAt = createdAtUtc
        };

        team.ApplyProfile(profile ?? TeamProfile.Empty);
        return team;
    }

    public void Update(
        string name,
        string coach,
        string city,
        TeamClassification classification,
        TeamProfile profile)
    {
        ArgumentNullException.ThrowIfNull(classification);
        ArgumentNullException.ThrowIfNull(profile);

        Name = ValidateName(name);
        Coach = ValidateCoach(coach);
        City = ValidateCity(city);
        Classification = classification;
        ApplyProfile(profile);
    }

    public void Activate() => Active = true;

    public void SetCrest(byte[] image, string contentType)
    {
        if (image is null || image.Length == 0)
            throw new ArgumentException("Crest image cannot be empty.", nameof(image));

        if (image.Length > MaxCrestBytes)
            throw new ArgumentException($"Crest image cannot exceed {MaxCrestBytes} bytes.", nameof(image));

        Crest = image;
        CrestContentType = ValidateCrestContentType(contentType);
    }

    private void ApplyProfile(TeamProfile profile)
    {
        ShortName = TrimOptional(profile.ShortName, MaxShortNameLength, "Short name");
        FoundedYear = ValidateFoundedYear(profile.FoundedYear);
        HomeVenue = profile.HomeVenue;
        Colors = profile.Colors;
        ContactEmail = ValidateContactEmail(profile.ContactEmail);
        ContactPhone = TrimOptional(profile.ContactPhone, MaxContactPhoneLength, "Contact phone");
        Website = ValidateWebsite(profile.Website);
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Team name is required.", nameof(name));

        return name.Trim();
    }

    private static string ValidateCoach(string coach)
    {
        if (string.IsNullOrWhiteSpace(coach))
            throw new ArgumentException("Team coach is required.", nameof(coach));

        return coach.Trim();
    }

    private static string ValidateCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("Team city is required.", nameof(city));

        return city.Trim();
    }

    private static string? TrimOptional(string? value, int maxLength, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"{label} cannot exceed {maxLength} characters.", nameof(value));

        return trimmed;
    }

    private int? ValidateFoundedYear(int? foundedYear)
    {
        if (foundedYear is null)
            return null;

        if (foundedYear < MinFoundedYear || foundedYear > CreatedAt.Year)
            throw new ArgumentException(
                $"Founded year must be between {MinFoundedYear} and {CreatedAt.Year}.", nameof(foundedYear));

        return foundedYear;
    }

    private static string? ValidateContactEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalized = email.Trim().ToLowerInvariant();
        if (normalized.Length > MaxContactEmailLength)
            throw new ArgumentException(
                $"Contact email cannot exceed {MaxContactEmailLength} characters.", nameof(email));

        if (!normalized.Contains('@'))
            throw new ArgumentException("Contact email is not a valid email address.", nameof(email));

        return normalized;
    }

    private static string? ValidateWebsite(string? website)
    {
        if (string.IsNullOrWhiteSpace(website))
            return null;

        var trimmed = website.Trim();
        if (trimmed.Length > MaxWebsiteLength)
            throw new ArgumentException($"Website cannot exceed {MaxWebsiteLength} characters.", nameof(website));

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("Website must be an absolute http(s) URL.", nameof(website));

        return trimmed;
    }

    private static string ValidateCrestContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Crest content type is required.", nameof(contentType));

        var normalized = contentType.Trim().ToLowerInvariant();
        if (!AllowedCrestContentTypes.Contains(normalized))
            throw new ArgumentException(
                $"Crest content type '{contentType}' is not an accepted image type.", nameof(contentType));

        return normalized;
    }
}
