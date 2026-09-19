namespace FairPlay.Sports.Domain.Teams;

/// <summary>
/// How and where a team competes. The three facets always travel together, so
/// they are one value object instead of three loose parameters. Immutable;
/// equality is by value.
/// </summary>
public sealed record TeamClassification
{
    public FootballType Type { get; }
    public Division? Division { get; }
    public AgeCategory Category { get; }

    /// <summary>
    /// <paramref name="division"/> is required for every category except
    /// <see cref="AgeCategory.Aficionados"/>, which doesn't compete in
    /// divisions - it must be null there and nowhere else.
    /// </summary>
    public TeamClassification(FootballType type, Division? division, AgeCategory category)
    {
        Type = ValidateEnum(type, nameof(type));
        Category = ValidateEnum(category, nameof(category));

        if (Category == AgeCategory.Aficionados)
        {
            if (division is not null)
                throw new ArgumentException("Aficionados teams don't have a division.", nameof(division));
            Division = null;
        }
        else
        {
            if (division is null)
                throw new ArgumentException("Division is required for this category.", nameof(division));
            Division = ValidateEnum(division.Value, nameof(division));
        }
    }

    private static TEnum ValidateEnum<TEnum>(TEnum value, string paramName) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new ArgumentException($"'{value}' is not a valid {typeof(TEnum).Name}.", paramName);

        return value;
    }
}
