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
    string? ContactEmail,
    string? ContactPhone,
    string? Website)
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
            team.ContactEmail,
            team.ContactPhone,
            team.Website);
}
