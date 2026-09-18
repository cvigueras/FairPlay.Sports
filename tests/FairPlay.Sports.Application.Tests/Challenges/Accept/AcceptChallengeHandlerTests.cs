using FairPlay.Sports.Application.Challenges;
using FairPlay.Sports.Application.Challenges.Accept;
using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Domain.Challenges;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.TestSupport.Challenges;
using FairPlay.Sports.TestSupport.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Challenges.Accept;

[TestFixture]
public class AcceptChallengeHandlerTests
{
    private static readonly DateTime Now = new(2026, 9, 18, 15, 0, 0, DateTimeKind.Utc);

    private IChallengeRepository _challenges = null!;
    private ITeamRepository _teams = null!;
    private ITeamMemberRepository _members = null!;
    private IClock _clock = null!;
    private AcceptChallengeHandler _handler = null!;
    private Challenge _challenge = null!;

    [SetUp]
    public void SetUp()
    {
        _challenge = ChallengeMother.DomainChallenge();
        _challenges = Substitute.For<IChallengeRepository>();
        _challenges.GetByIdForUpdateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(_challenge);
        _teams = Substitute.For<ITeamRepository>();
        _teams.GetByIdAsync(ChallengeMother.ChallengerTeamId, Arg.Any<CancellationToken>())
            .Returns(TeamMother.DomainTeam(id: ChallengeMother.ChallengerTeamId));
        _teams.GetByIdAsync(ChallengeMother.ChallengedTeamId, Arg.Any<CancellationToken>())
            .Returns(TeamMother.DomainTeam(id: ChallengeMother.ChallengedTeamId));
        _members = Substitute.For<ITeamMemberRepository>();
        _members.GetByTeamAndUserAsync(
                ChallengeMother.ChallengedTeamId, ChallengeMother.ActingUserId, Arg.Any<CancellationToken>())
            .Returns(ChallengeMother.Member(ChallengeMother.ChallengedTeamId, ChallengeMother.ActingUserId, TeamMemberRole.Coach));
        _clock = Substitute.For<IClock>();
        _clock.UtcNow.Returns(Now);
        _handler = new AcceptChallengeHandler(_challenges, _teams, _members, _clock);
    }

    [Test]
    public async Task Handle_WhenChallengeDoesNotExist_ReturnsNotFound()
    {
        _challenges.GetByIdForUpdateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Challenge?)null);

        var result = await _handler.Handle(ChallengeMother.AcceptCommand(), CancellationToken.None);

        Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
    }

    [Test]
    public async Task Handle_WhenActingUserIsNotAMemberOfTheChallengedTeam_ReturnsFailure()
    {
        _members.GetByTeamAndUserAsync(
                ChallengeMother.ChallengedTeamId, ChallengeMother.ActingUserId, Arg.Any<CancellationToken>())
            .Returns((TeamMember?)null);

        var result = await _handler.Handle(ChallengeMother.AcceptCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ChallengeMother.NotAllowedToRespond));
        });
        Assert.That(_challenge.Status, Is.EqualTo(ChallengeStatus.Pending));
    }

    [Test]
    public async Task Handle_WhenAlreadyResponded_ReturnsFailure()
    {
        _challenge.Accept(Now);

        var result = await _handler.Handle(ChallengeMother.AcceptCommand(), CancellationToken.None);

        Assert.That(result.Error, Is.EqualTo(ChallengeMother.AlreadyResponded));
    }

    [Test]
    public async Task Handle_OnSuccess_AcceptsTheChallenge_AndReturnsDto()
    {
        var result = await _handler.Handle(ChallengeMother.AcceptCommand(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(_challenge.Status, Is.EqualTo(ChallengeStatus.Accepted));
            Assert.That(_challenge.RespondedAt, Is.EqualTo(Now));
            Assert.That(result.Value!.Status, Is.EqualTo(ChallengeStatus.Accepted));
        });
    }
}
