using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Teams;

public sealed record TeamDto(
    Guid Id,
    string Name,
    string Coach,
    string City,
    FootballType Type,
    Division Division,
    AgeCategory Category,
    bool HasCrest,
    DateTime CreatedAt,
    bool Active,
    string? ShortName,
    int? FoundedYear,
    string? VenueName,
    string? VenueAddress,
    PitchSurface? VenueSurface,
    string? VenueMapsUrl,
    string? ColorPrimary,
    string? ColorSecondary,
    KitPattern? KitPattern,
    string? ContactEmail,
    string? ContactPhone,
    string? Website,
    string? AlternateColorPrimary,
    string? AlternateColorSecondary,
    KitPattern? AlternateKitPattern)
{
    public static TeamDto FromDomain(Team team) =>
        new(
            team.Id,
            team.Name,
            team.Coach,
            team.City,
            team.Classification.Type,
            team.Classification.Division,
            team.Classification.Category,
            team.HasCrest,
            team.CreatedAt,
            team.Active,
            team.ShortName,
            team.FoundedYear,
            team.HomeVenue?.Name,
            team.HomeVenue?.Address,
            team.HomeVenue?.Surface,
            team.HomeVenue?.MapsUrl,
            team.Colors?.Primary,
            team.Colors?.Secondary,
            team.Colors?.Pattern,
            team.ContactEmail,
            team.ContactPhone,
            team.Website,
            team.AlternateColors?.Primary,
            team.AlternateColors?.Secondary,
            team.AlternateColors?.Pattern);
}
