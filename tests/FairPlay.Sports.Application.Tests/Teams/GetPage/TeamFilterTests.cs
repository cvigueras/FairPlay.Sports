using FairPlay.Sports.Application.Teams.GetPage;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.TestSupport.Teams;

namespace FairPlay.Sports.Application.Tests.Teams.GetPage;

[TestFixture]
public class TeamFilterTests
{
    private static IQueryable<Team> Teams() => new[]
    {
        TeamMother.DomainTeam(name: "Real Betis", city: "Sevilla", type: FootballType.Football11, active: true),
        TeamMother.DomainTeam(name: "Sevilla FC", city: "Sevilla", type: FootballType.Futsal, active: false),
        TeamMother.DomainTeam(name: "Cadiz CF", city: "Cadiz", type: FootballType.Football11, active: true)
    }.AsQueryable();

    [Test]
    public void EmptyFilter_returnsEverything()
    {
        var result = new TeamFilter().Apply(Teams());

        Assert.That(result.Count(), Is.EqualTo(3));
    }

    [Test]
    public void Name_matchesCaseInsensitiveContains()
    {
        var result = new TeamFilter(Name: "SEV").Apply(Teams());

        Assert.That(result.Select(t => t.Name), Is.EquivalentTo(new[] { "Sevilla FC" }));
    }

    [Test]
    public void City_matchesCaseInsensitiveContains()
    {
        var result = new TeamFilter(City: "sevilla").Apply(Teams());

        Assert.That(result.Select(t => t.Name), Is.EquivalentTo(new[] { "Real Betis", "Sevilla FC" }));
    }

    [Test]
    public void Type_matchesExactly()
    {
        var result = new TeamFilter(Type: FootballType.Football11).Apply(Teams());

        Assert.That(result.Select(t => t.Name), Is.EquivalentTo(new[] { "Real Betis", "Cadiz CF" }));
    }

    [Test]
    public void Active_matchesExactly()
    {
        var result = new TeamFilter(Active: false).Apply(Teams());

        Assert.That(result.Select(t => t.Name), Is.EquivalentTo(new[] { "Sevilla FC" }));
    }

    [Test]
    public void Predicates_areCombinedWithAnd()
    {
        var result = new TeamFilter(City: "Sevilla", Type: FootballType.Football11).Apply(Teams());

        Assert.That(result.Select(t => t.Name), Is.EquivalentTo(new[] { "Real Betis" }));
    }
}
