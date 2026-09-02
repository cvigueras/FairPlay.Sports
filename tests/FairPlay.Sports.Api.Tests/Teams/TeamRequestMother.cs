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
}
