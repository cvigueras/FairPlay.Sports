using FairPlay.Sports.Domain.Standings;

namespace FairPlay.Sports.Domain.Tests.Standings;

[TestFixture]
public class StandingTests
{
    private static readonly DateTime Now = new(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc);

    private static Standing Create(
        Guid? id = null,
        Guid? teamId = null,
        int points = 10, int played = 5, int won = 3, int drawn = 1, int lost = 1,
        int goalsFor = 12, int goalsAgainst = 7) =>
        Standing.Create(
            id ?? Guid.NewGuid(), teamId ?? Guid.NewGuid(),
            points, played, won, drawn, lost, goalsFor, goalsAgainst, Now);

    [Test]
    public void Create_setsEveryField()
    {
        var teamId = Guid.NewGuid();

        var standing = Create(teamId: teamId, points: 10, played: 5, won: 3, drawn: 1, lost: 1, goalsFor: 12, goalsAgainst: 7);

        Assert.Multiple(() =>
        {
            Assert.That(standing.TeamId, Is.EqualTo(teamId));
            Assert.That(standing.Points, Is.EqualTo(10));
            Assert.That(standing.Played, Is.EqualTo(5));
            Assert.That(standing.Won, Is.EqualTo(3));
            Assert.That(standing.Drawn, Is.EqualTo(1));
            Assert.That(standing.Lost, Is.EqualTo(1));
            Assert.That(standing.GoalsFor, Is.EqualTo(12));
            Assert.That(standing.GoalsAgainst, Is.EqualTo(7));
            Assert.That(standing.CreatedAt, Is.EqualTo(Now));
        });
    }

    [Test]
    public void Create_withEmptyId_throws() =>
        Assert.That(() => Standing.Create(Guid.Empty, Guid.NewGuid(), 0, 0, 0, 0, 0, 0, 0, Now), Throws.ArgumentException);

    [Test]
    public void Create_withEmptyTeamId_throws() =>
        Assert.That(() => Standing.Create(Guid.NewGuid(), Guid.Empty, 0, 0, 0, 0, 0, 0, 0, Now), Throws.ArgumentException);

    [TestCase(-1, 0, 0, 0, 0, 0, 0)]
    [TestCase(0, -1, 0, 0, 0, 0, 0)]
    [TestCase(0, 0, -1, 0, 0, 0, 0)]
    [TestCase(0, 0, 0, -1, 0, 0, 0)]
    [TestCase(0, 0, 0, 0, -1, 0, 0)]
    [TestCase(0, 0, 0, 0, 0, -1, 0)]
    [TestCase(0, 0, 0, 0, 0, 0, -1)]
    public void Create_withAnyNegativeStat_throws(
        int points, int played, int won, int drawn, int lost, int goalsFor, int goalsAgainst) =>
        Assert.That(
            () => Create(points: points, played: played, won: won, drawn: drawn, lost: lost, goalsFor: goalsFor, goalsAgainst: goalsAgainst),
            Throws.ArgumentException);

    [Test]
    public void Create_whenPlayedDoesNotEqualWonDrawnLost_throws() =>
        Assert.That(() => Create(played: 10, won: 3, drawn: 1, lost: 1), Throws.ArgumentException);

    [Test]
    public void UpdateStats_replacesEveryStat()
    {
        var standing = Create();

        standing.UpdateStats(points: 20, played: 8, won: 6, drawn: 1, lost: 1, goalsFor: 18, goalsAgainst: 9);

        Assert.Multiple(() =>
        {
            Assert.That(standing.Points, Is.EqualTo(20));
            Assert.That(standing.Played, Is.EqualTo(8));
            Assert.That(standing.Won, Is.EqualTo(6));
            Assert.That(standing.Drawn, Is.EqualTo(1));
            Assert.That(standing.Lost, Is.EqualTo(1));
            Assert.That(standing.GoalsFor, Is.EqualTo(18));
            Assert.That(standing.GoalsAgainst, Is.EqualTo(9));
        });
    }

    [Test]
    public void UpdateStats_whenPlayedDoesNotEqualWonDrawnLost_throws_andLeavesStatsUnchanged()
    {
        var standing = Create();

        Assert.That(() => standing.UpdateStats(0, played: 10, won: 3, drawn: 1, lost: 1, goalsFor: 0, goalsAgainst: 0), Throws.ArgumentException);
        Assert.That(standing.Played, Is.EqualTo(5));
    }
}
