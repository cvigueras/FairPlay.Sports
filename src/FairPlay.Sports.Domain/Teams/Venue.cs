namespace FairPlay.Sports.Domain.Teams;

/// <summary>
/// A team's home venue as it appears on a federation club sheet: the ground's
/// name, its street address, its playing surface and an optional "how to get
/// there" map link. The four facets travel together, so they are one value
/// object instead of loose fields. Immutable; equality is by value.
/// </summary>
public sealed record Venue
{
    public const int MaxNameLength = 150;
    public const int MaxAddressLength = 300;
    public const int MaxMapsUrlLength = 2048;

    public string Name { get; }
    public string Address { get; }
    public PitchSurface Surface { get; }
    public string? MapsUrl { get; }

    public Venue(string name, string address, PitchSurface surface, string? mapsUrl = null)
    {
        Name = Require(name, nameof(name), "Venue name", MaxNameLength);
        Address = Require(address, nameof(address), "Venue address", MaxAddressLength);

        if (!Enum.IsDefined(surface) || surface == PitchSurface.Default)
            throw new ArgumentException($"'{surface}' is not a valid {nameof(PitchSurface)}.", nameof(surface));
        Surface = surface;

        MapsUrl = NormalizeMapsUrl(mapsUrl);
    }

    private static string Require(string value, string paramName, string label, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{label} is required.", paramName);

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"{label} cannot exceed {maxLength} characters.", paramName);

        return trimmed;
    }

    private static string? NormalizeMapsUrl(string? mapsUrl)
    {
        if (string.IsNullOrWhiteSpace(mapsUrl))
            return null;

        var trimmed = mapsUrl.Trim();
        if (trimmed.Length > MaxMapsUrlLength)
            throw new ArgumentException($"Maps link cannot exceed {MaxMapsUrlLength} characters.", nameof(mapsUrl));

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("Maps link must be an absolute http(s) URL.", nameof(mapsUrl));

        return trimmed;
    }
}
