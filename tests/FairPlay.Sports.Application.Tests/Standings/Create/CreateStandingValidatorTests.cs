using FairPlay.Sports.Application.Standings.Create;

namespace FairPlay.Sports.Application.Tests.Standings.Create;

[TestFixture]
public class CreateStandingValidatorTests
{
    private readonly CreateStandingValidator _validator = new();

    private static CreateStandingCommand Command(
        Guid? teamId = null,
        int points = 10, int played = 5, int won = 3, int drawn = 1, int lost = 1,
        int goalsFor = 12, int goalsAgainst = 7) =>
        new(teamId ?? Guid.NewGuid(), points, played, won, drawn, lost, goalsFor, goalsAgainst);

    [Test]
    public void Passes_forSaneStats()
    {
        Assert.That(_validator.Validate(Command()).IsValid, Is.True);
    }

    [Test]
    public void Fails_whenTeamIdIsEmpty()
    {
        Assert.That(_validator.Validate(Command(teamId: Guid.Empty)).IsValid, Is.False);
    }

    [TestCase(-1, 0, 0, 0, 0)]
    [TestCase(0, -1, 0, 0, 0)]
    [TestCase(0, 0, -1, 0, 0)]
    [TestCase(0, 0, 0, -1, 0)]
    [TestCase(0, 0, 0, 0, -1)]
    public void Fails_whenAnyStatIsNegative(int points, int won, int drawn, int lost, int goalsFor)
    {
        var command = Command(points: points, played: 0, won: won, drawn: drawn, lost: lost, goalsFor: goalsFor);

        Assert.That(_validator.Validate(command).IsValid, Is.False);
    }

    [Test]
    public void Fails_whenPlayedDoesNotEqualWonDrawnLost()
    {
        var command = Command(played: 10, won: 3, drawn: 1, lost: 1);

        Assert.That(_validator.Validate(command).IsValid, Is.False);
    }

    [Test]
    public void Passes_whenPlayedIsZeroAndAllStatsAreZero()
    {
        var command = Command(points: 0, played: 0, won: 0, drawn: 0, lost: 0, goalsFor: 0, goalsAgainst: 0);

        Assert.That(_validator.Validate(command).IsValid, Is.True);
    }
}
