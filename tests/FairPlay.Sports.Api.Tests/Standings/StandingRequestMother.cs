using FairPlay.Sports.Api.Standings;
using FairPlay.Sports.TestSupport.Standings;

namespace FairPlay.Sports.Api.Tests.Standings;

internal static class StandingRequestMother
{
    public static CreateStandingRequest CreateRequest(Guid? teamId = null) =>
        new(
            teamId ?? Guid.NewGuid(),
            StandingMother.Points,
            StandingMother.Played,
            StandingMother.Won,
            StandingMother.Drawn,
            StandingMother.Lost,
            StandingMother.GoalsFor,
            StandingMother.GoalsAgainst);

    public static UpdateStandingRequest UpdateRequest() =>
        new(
            StandingMother.Points,
            StandingMother.Played,
            StandingMother.Won,
            StandingMother.Drawn,
            StandingMother.Lost,
            StandingMother.GoalsFor,
            StandingMother.GoalsAgainst);
}
