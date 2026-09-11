using FairPlay.Sports.Application.Standings.GetPage;
using FairPlay.Sports.Domain.Standings;
using FairPlay.Sports.TestSupport.Standings;

namespace FairPlay.Sports.Application.Tests.Standings.GetPage;

[TestFixture]
public class StandingSortTests
{
    private static IQueryable<Standing> Standings() => new[]
    {
        StandingMother.DomainStanding(points: 10, played: 4, won: 3, drawn: 1, lost: 0, goalsFor: 8, goalsAgainst: 3),
        StandingMother.DomainStanding(points: 15, played: 5, won: 5, drawn: 0, lost: 0, goalsFor: 12, goalsAgainst: 2),
        StandingMother.DomainStanding(points: 6, played: 3, won: 2, drawn: 0, lost: 1, goalsFor: 5, goalsAgainst: 4)
    }.AsQueryable();

    [Test]
    public void NoSort_fallsBackToPointsDescending()
    {
        var result = new StandingSort(null).Apply(Standings());

        Assert.That(result.Select(s => s.Points), Is.EqualTo(new[] { 15, 10, 6 }));
    }

    [Test]
    public void Ascending_sortsPointsLowestFirst()
    {
        var result = new StandingSort("points").Apply(Standings());

        Assert.That(result.Select(s => s.Points), Is.EqualTo(new[] { 6, 10, 15 }));
    }

    [Test]
    public void SortsByGoalDifference()
    {
        var result = new StandingSort("-goaldifference").Apply(Standings());

        // Differences: 5, 10, 1 -> descending: 15pts(10), 10pts(5), 6pts(1)
        Assert.That(result.Select(s => s.GoalsFor - s.GoalsAgainst), Is.EqualTo(new[] { 10, 5, 1 }));
    }

    [Test]
    public void UnknownField_isIgnored_leavingTheDefaultOrder()
    {
        var result = new StandingSort("unknown").Apply(Standings());

        Assert.That(result.Select(s => s.Points), Is.EqualTo(new[] { 15, 10, 6 }));
    }
}
