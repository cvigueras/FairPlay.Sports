using FairPlay.Sports.Api.Teams;
using FairPlay.Sports.TestSupport.Teams;

namespace FairPlay.Sports.Api.Tests.Teams;

internal static class TeamRequestMother
{
    public static CreateTeamRequest CreateRequest() =>
        new(
            TeamMother.Name,
            TeamMother.Coach,
            TeamMother.City,
            TeamMother.DefaultType,
            TeamMother.DefaultDivision,
            TeamMother.DefaultCategory);

    public static CreateTeamRequest CreateRequestWithProfile() =>
        new(
            TeamMother.Name,
            TeamMother.Coach,
            TeamMother.City,
            TeamMother.DefaultType,
            TeamMother.DefaultDivision,
            TeamMother.DefaultCategory,
            TeamMother.ShortName,
            TeamMother.FoundedYear,
            TeamMother.VenueName,
            TeamMother.VenueAddress,
            TeamMother.VenueSurface,
            TeamMother.VenueMapsUrl,
            TeamMother.ColorPrimary,
            TeamMother.ColorSecondary,
            TeamMother.ContactEmail,
            TeamMother.ContactPhone,
            TeamMother.Website);

    public static UpdateTeamRequest UpdateRequest() =>
        new(
            TeamMother.Name,
            TeamMother.Coach,
            TeamMother.City,
            TeamMother.DefaultType,
            TeamMother.DefaultDivision,
            TeamMother.DefaultCategory,
            TeamMother.ShortName,
            TeamMother.FoundedYear,
            TeamMother.VenueName,
            TeamMother.VenueAddress,
            TeamMother.VenueSurface,
            TeamMother.VenueMapsUrl,
            TeamMother.ColorPrimary,
            TeamMother.ColorSecondary,
            TeamMother.ContactEmail,
            TeamMother.ContactPhone,
            TeamMother.Website);
}
