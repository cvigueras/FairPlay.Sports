using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Teams;

/// <summary>
/// Turns the flat command fields into the domain shapes. Runs after the
/// validator has already gated the input, so the value-object constructors
/// here never throw for a well-formed request.
/// </summary>
internal static class TeamWriteMapping
{
    public static TeamClassification ToClassification(this ITeamWriteFields fields) =>
        new(fields.Type, fields.Division, fields.Category);

    public static TeamProfile ToProfile(this ITeamWriteFields fields) =>
        new(
            ShortName: fields.ShortName,
            FoundedYear: fields.FoundedYear,
            HomeVenue: BuildVenue(fields),
            Colors: BuildColors(fields),
            ContactEmail: fields.ContactEmail,
            ContactPhone: fields.ContactPhone,
            Website: fields.Website);

    private static Venue? BuildVenue(ITeamWriteFields fields)
    {
        if (string.IsNullOrWhiteSpace(fields.VenueName) &&
            string.IsNullOrWhiteSpace(fields.VenueAddress) &&
            fields.VenueSurface is null or PitchSurface.Default &&
            string.IsNullOrWhiteSpace(fields.VenueMapsUrl))
        {
            return null;
        }

        return new Venue(
            fields.VenueName!,
            fields.VenueAddress!,
            fields.VenueSurface ?? PitchSurface.Default,
            fields.VenueMapsUrl);
    }

    private static KitColors? BuildColors(ITeamWriteFields fields)
    {
        if (string.IsNullOrWhiteSpace(fields.ColorPrimary) && string.IsNullOrWhiteSpace(fields.ColorSecondary))
            return null;

        return new KitColors(fields.ColorPrimary!, fields.ColorSecondary!);
    }
}
