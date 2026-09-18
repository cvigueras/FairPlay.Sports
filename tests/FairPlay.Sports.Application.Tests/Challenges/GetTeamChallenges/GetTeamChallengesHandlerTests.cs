using FairPlay.Sports.Application.Challenges;
using FairPlay.Sports.Application.Challenges.GetTeamChallenges;
using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Domain.Challenges;
using FairPlay.Sports.TestSupport.Challenges;
using FairPlay.Sports.TestSupport.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Challenges.GetTeamChallenges;

[TestFixture]
public class GetTeamChallengesHandlerTests
{
    private IChallengeRepository _challenges = null!;
    private ITeamRepository _teams = null!;
    private GetTeamChallengesHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _challenges = Substitute.For<IChallengeRepository>();
        _teams = Substitute.For<ITeamRepository>();
        _teams.ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        _handler = new GetTeamChallengesHandler(_challenges, _teams);
    }

    [Test]
    public async Task Handle_WhenTeamDoesNotExist_ReturnsNotFound()
    {
        _teams.ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(new GetTeamChallengesQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
    }

    [Test]
    public async Task Handle_ReturnsSentAndReceivedChallenges_NewestFirst()
    {
        var thirdTeamId = Guid.NewGuid();
        var older = ChallengeMother.DomainChallenge(
            challengerTeamId: ChallengeMother.ChallengerTeamId,
            challengedTeamId: ChallengeMother.ChallengedTeamId,
            createdAtUtc: new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc));
        var newer = ChallengeMother.DomainChallenge(
            challengerTeamId: thirdTeamId,
            challengedTeamId: ChallengeMother.ChallengerTeamId,
            createdAtUtc: new DateTime(2026, 9, 10, 0, 0, 0, DateTimeKind.Utc));
        _challenges.GetByTeamIdAsync(ChallengeMother.ChallengerTeamId, Arg.Any<CancellationToken>())
            .Returns([older, newer]);
        _teams.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                TeamMother.DomainTeamWithProfile(id: ChallengeMother.ChallengerTeamId),
                TeamMother.DomainTeamWithProfile(id: ChallengeMother.ChallengedTeamId),
                TeamMother.DomainTeamWithProfile(id: thirdTeamId),
            ]);

        var result = await _handler.Handle(
            new GetTeamChallengesQuery(ChallengeMother.ChallengerTeamId), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value!.Select(dto => dto.Id), Is.EqualTo(new[] { newer.Id, older.Id }));
    }

    [Test]
    public async Task Handle_WhenNoChallenges_ReturnsEmptyList()
    {
        _challenges.GetByTeamIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Challenge>());

        var result = await _handler.Handle(new GetTeamChallengesQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.That(result.Value, Is.Empty);
    }
}
