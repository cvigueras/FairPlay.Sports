namespace FairPlay.Sports.Domain.Teams;

public sealed class Team
{
    /// <summary>Upper bound for a stored crest image.</summary>
    public const int MaxCrestBytes = 2 * 1024 * 1024;

    private static readonly string[] AllowedCrestContentTypes =
        ["image/png", "image/jpeg", "image/webp", "image/svg+xml"];

    public Guid Id { get; }
    public string Name { get; private set; }
    public string Coach { get; private set; }
    public string City { get; private set; }
    public FootballType Type { get; private set; }
    public Division Division { get; private set; }
    public AgeCategory Category { get; private set; }
    public byte[]? Crest { get; private set; }
    public string? CrestContentType { get; private set; }
    public DateTime CreatedAt { get; }
    public bool Active { get; private set; }

    public bool HasCrest => Crest is { Length: > 0 };

    private Team(
        Guid id,
        string name,
        string coach,
        string city,
        FootballType type,
        Division division,
        AgeCategory category,
        DateTime createdAt,
        bool active)
    {
        Id = id;
        Name = name;
        Coach = coach;
        City = city;
        Type = type;
        Division = division;
        Category = category;
        CreatedAt = createdAt;
        Active = active;
    }

    public static Team Create(
        Guid id,
        string name,
        string coach,
        string city,
        FootballType type,
        Division division,
        AgeCategory category,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(id));

        return new(
            id,
            ValidateName(name),
            ValidateCoach(coach),
            ValidateCity(city),
            ValidateEnum(type, nameof(type)),
            ValidateEnum(division, nameof(division)),
            ValidateEnum(category, nameof(category)),
            createdAtUtc,
            active: true);
    }

    public void Rename(string name) => Name = ValidateName(name);

    public void ChangeCoach(string coach) => Coach = ValidateCoach(coach);

    public void Relocate(string city) => City = ValidateCity(city);

    public void ChangeType(FootballType type) => Type = ValidateEnum(type, nameof(type));

    public void MoveToDivision(Division division) => Division = ValidateEnum(division, nameof(division));

    public void ChangeCategory(AgeCategory category) => Category = ValidateEnum(category, nameof(category));

    public void Deactivate() => Active = false;

    public void Reactivate() => Active = true;

    public void SetCrest(byte[] image, string contentType)
    {
        if (image is null || image.Length == 0)
            throw new ArgumentException("Crest image cannot be empty.", nameof(image));

        if (image.Length > MaxCrestBytes)
            throw new ArgumentException($"Crest image cannot exceed {MaxCrestBytes} bytes.", nameof(image));

        Crest = image;
        CrestContentType = ValidateCrestContentType(contentType);
    }

    public void RemoveCrest()
    {
        Crest = null;
        CrestContentType = null;
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

    private static TEnum ValidateEnum<TEnum>(TEnum value, string paramName) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new ArgumentException($"'{value}' is not a valid {typeof(TEnum).Name}.", paramName);

        return value;
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
