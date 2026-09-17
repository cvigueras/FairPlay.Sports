using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Api.Teams;

public sealed record CreateTeamRequest(
    string Name,
    string Coach,
    string City,
    FootballType Type = default,
    Division Division = default,
    AgeCategory Category = default,
    string? ShortName = null,
    int? FoundedYear = null,
    string? VenueName = null,
    string? VenueAddress = null,
    PitchSurface? VenueSurface = null,
    string? VenueMapsUrl = null,
    string? ColorPrimary = null,
    string? ColorSecondary = null,
    KitPattern? KitPattern = null,
    string? ContactEmail = null,
    string? ContactPhone = null,
    string? Website = null,
    string? AlternateColorPrimary = null,
    string? AlternateColorSecondary = null,
    KitPattern? AlternateKitPattern = null,
    string? ShortsColor = null,
    string? AlternateShortsColor = null);
