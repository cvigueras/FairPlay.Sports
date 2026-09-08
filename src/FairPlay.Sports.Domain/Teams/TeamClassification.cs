namespace FairPlay.Sports.Domain.Teams;

/// <summary>
/// How and where a team competes. The three facets always travel together, so
/// they are one value object instead of three loose parameters. Immutable;
/// equality is by value.
/// </summary>
public sealed record TeamClassification
{
    public FootballType Type { get; }
    public Division Division { get; }
    public AgeCategory Category { get; }

    public TeamClassification(FootballType type, Division division, AgeCategory category)
    {
        Type = ValidateEnum(type, nameof(type));
        Division = ValidateEnum(division, nameof(division));
        Category = ValidateEnum(category, nameof(category));
    }

    private static TEnum ValidateEnum<TEnum>(TEnum value, string paramName) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new ArgumentException($"'{value}' is not a valid {typeof(TEnum).Name}.", paramName);

        return value;
    }
}
