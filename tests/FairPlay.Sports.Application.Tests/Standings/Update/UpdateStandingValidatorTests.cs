using FairPlay.Sports.Application.Standings.Update;

namespace FairPlay.Sports.Application.Tests.Standings.Update;

[TestFixture]
public class UpdateStandingValidatorTests
{
    private readonly UpdateStandingValidator _validator = new();

    private static UpdateStandingCommand Command(
        int points = 10, int played = 5, int won = 3, int drawn = 1, int lost = 1,
        int goalsFor = 12, int goalsAgainst = 7) =>
        new(Guid.NewGuid(), points, played, won, drawn, lost, goalsFor, goalsAgainst);

    [Test]
    public void Passes_forSaneStats()
    {
        Assert.That(_validator.Validate(Command()).IsValid, Is.True);
    }

    [Test]
    public void Fails_whenAStatIsNegative()
    {
        Assert.That(_validator.Validate(Command(goalsAgainst: -1)).IsValid, Is.False);
    }

    [Test]
    public void Fails_whenPlayedDoesNotEqualWonDrawnLost()
    {
        Assert.That(_validator.Validate(Command(played: 1, won: 3, drawn: 1, lost: 1)).IsValid, Is.False);
    }
}
