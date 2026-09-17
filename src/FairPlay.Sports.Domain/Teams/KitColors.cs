namespace FairPlay.Sports.Domain.Teams;

/// <summary>
/// A club's kit colours as listed on a federation club sheet: the primary
/// and secondary shirt colours, the pattern that lays them out on the shirt,
/// and the shorts colour (shorts are always plain - one solid colour, no
/// pattern). Colours are stored as free text (a colour name or hex) since
/// federations do not standardise them. Immutable; equality is by value.
/// </summary>
public sealed record KitColors
{
    public const int MaxColourLength = 50;

    public string Primary { get; }
    public string Secondary { get; }
    public string ShortsColor { get; }
    public KitPattern Pattern { get; }

    public KitColors(string primary, string secondary, string shortsColor, KitPattern pattern)
    {
        Primary = Require(primary, nameof(primary), "Primary kit colour");
        Secondary = Require(secondary, nameof(secondary), "Secondary kit colour");
        ShortsColor = Require(shortsColor, nameof(shortsColor), "Shorts colour");

        if (!Enum.IsDefined(pattern) || pattern == KitPattern.Default)
            throw new ArgumentException($"'{pattern}' is not a valid {nameof(KitPattern)}.", nameof(pattern));
        Pattern = pattern;
    }

    private static string Require(string value, string paramName, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{label} is required.", paramName);

        var trimmed = value.Trim();
        if (trimmed.Length > MaxColourLength)
            throw new ArgumentException($"{label} cannot exceed {MaxColourLength} characters.", paramName);

        return trimmed;
    }
}
