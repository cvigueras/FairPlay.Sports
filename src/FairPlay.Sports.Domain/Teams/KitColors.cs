namespace FairPlay.Sports.Domain.Teams;

/// <summary>
/// A club's kit colours as listed on a federation club sheet: the primary
/// ("1ª equipación") and secondary ("2ª equipación") shirt colours. Stored as
/// free text (a colour name or hex) since federations do not standardise them.
/// Immutable; equality is by value.
/// </summary>
public sealed record KitColors
{
    public const int MaxColourLength = 50;

    public string Primary { get; }
    public string Secondary { get; }

    public KitColors(string primary, string secondary)
    {
        Primary = Require(primary, nameof(primary), "Primary kit colour");
        Secondary = Require(secondary, nameof(secondary), "Secondary kit colour");
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
