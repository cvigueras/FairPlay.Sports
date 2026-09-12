using FairPlay.Sports.Application.Standings.GetPage;
using FairPlay.Sports.Domain.Standings;
using FairPlay.Sports.Domain.Teams;
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

    [Test]
    public void TeamIds_filtersToTheGivenSet()
    {
        var (teamA, teamB) = (Guid.NewGuid(), Guid.NewGuid());

        var result = new StandingFilter(TeamIds: [teamA]).Apply(Standings(teamA, teamB));

        Assert.That(result.Single().TeamId, Is.EqualTo(teamA));
    }

    [Test]
    public void Type_Division_Category_areNotAppliedDirectly()
    {
        // They are resolved to TeamIds by GetStandingsPageHandler before Apply runs - on their
        // own they must not filter anything out, or the handler's resolution would be bypassed.
        var (teamA, teamB) = (Guid.NewGuid(), Guid.NewGuid());

        var result = new StandingFilter(Type: FootballType.Futsal).Apply(Standings(teamA, teamB));

        Assert.That(result.Count(), Is.EqualTo(2));
    }
}
