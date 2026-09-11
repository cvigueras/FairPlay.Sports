using FairPlay.Sports.Application.Standings.GetPage;
using FairPlay.Sports.Domain.Standings;
using FairPlay.Sports.TestSupport.Standings;

namespace FairPlay.Sports.Application.Tests.Standings.GetPage;

[TestFixture]
public class StandingFilterTests
{
    private static IQueryable<Standing> Standings(Guid teamA, Guid teamB) => new[]
    {
        StandingMother.DomainStanding(teamId: teamA),
        StandingMother.DomainStanding(teamId: teamB)
    }.AsQueryable();

    [Test]
    public void EmptyFilter_returnsEverything()
    {
        var (teamA, teamB) = (Guid.NewGuid(), Guid.NewGuid());

        var result = new StandingFilter().Apply(Standings(teamA, teamB));

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public void TeamId_filtersByExactMatch()
    {
        var (teamA, teamB) = (Guid.NewGuid(), Guid.NewGuid());

        var result = new StandingFilter(TeamId: teamA).Apply(Standings(teamA, teamB));

        Assert.That(result.Single().TeamId, Is.EqualTo(teamA));
    }
}
