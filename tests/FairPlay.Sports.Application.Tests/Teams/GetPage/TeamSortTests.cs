using FairPlay.Sports.Application.Teams.GetPage;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.TestSupport.Teams;

namespace FairPlay.Sports.Application.Tests.Teams.GetPage;

[TestFixture]
public class TeamSortTests
{
    private static readonly DateTime Base = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static IQueryable<Team> Teams() => new[]
    {
        TeamMother.DomainTeam(name: "Bravo", city: "Zaragoza", createdAtUtc: Base.AddDays(2)),
        TeamMother.DomainTeam(name: "Alpha", city: "Zaragoza", createdAtUtc: Base.AddDays(1)),
        TeamMother.DomainTeam(name: "Charlie", city: "Almeria", createdAtUtc: Base.AddDays(3))
    }.AsQueryable();

    [Test]
    public void NoSort_fallsBackToNameAscending()
    {
        var result = new TeamSort(null).Apply(Teams());

        Assert.That(result.Select(t => t.Name), Is.EqualTo(new[] { "Alpha", "Bravo", "Charlie" }));
    }

    [Test]
    public void Descending_prefix_reversesOrder()
    {
        var result = new TeamSort("-name").Apply(Teams());

        Assert.That(result.Select(t => t.Name), Is.EqualTo(new[] { "Charlie", "Bravo", "Alpha" }));
    }

    [Test]
    public void SortsByCreatedAt()
    {
        var result = new TeamSort("-createdAt").Apply(Teams());

        Assert.That(result.Select(t => t.Name), Is.EqualTo(new[] { "Charlie", "Bravo", "Alpha" }));
    }

    [Test]
    public void CompositeSort_appliesThenBy_andEndsOnNameAsTieBreaker()
    {
        var result = new TeamSort("city").Apply(Teams());

        // Almeria first (Charlie), then Zaragoza ordered by the Name tie-breaker.
        Assert.That(result.Select(t => t.Name), Is.EqualTo(new[] { "Charlie", "Alpha", "Bravo" }));
    }

    [Test]
    public void UnknownField_isIgnored_leavingTheDefaultOrder()
    {
        var result = new TeamSort("coach").Apply(Teams());

        Assert.That(result.Select(t => t.Name), Is.EqualTo(new[] { "Alpha", "Bravo", "Charlie" }));
    }
}
