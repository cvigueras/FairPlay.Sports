using FairPlay.Sports.Application.Challenges;
using FairPlay.Sports.Application.Challenges.Send;
using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Domain.Challenges;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.TestSupport.Challenges;
using FairPlay.Sports.TestSupport.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Challenges.Send;

[TestFixture]
public class SendChallengeHandlerTests
{
    private static readonly DateTime Now = new(2026, 9, 18, 12, 0, 0, DateTimeKind.Utc);

    private IChallengeRepository _challenges = null!;
    private ITeamRepository _teams = null!;
    private ITeamMemberRepository _members = null!;
    private IClock _clock = null!;
    private SendChallengeHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _challenges = Substitute.For<IChallengeRepository>();
        _teams = Substitute.For<ITeamRepository>();
        _teams.GetByIdAsync(ChallengeMother.ChallengerTeamId, Arg.Any<CancellationToken>())
            .Returns(TeamMother.DomainTeam(id: ChallengeMother.ChallengerTeamId));
        _teams.GetByIdAsync(ChallengeMother.ChallengedTeamId, Arg.Any<CancellationToken>())
            .Returns(TeamMother.DomainTeam(id: ChallengeMother.ChallengedTeamId));
        _members = Substitute.For<ITeamMemberRepository>();
        _members.GetByTeamAndUserAsync(
                ChallengeMother.ChallengerTeamId, ChallengeMother.ActingUserId, Arg.Any<CancellationToken>())
            .Returns(ChallengeMother.Member(ChallengeMother.ChallengerTeamId, ChallengeMother.ActingUserId, TeamMemberRole.Delegate));
        _clock = Substitute.For<IClock>();
        _clock.UtcNow.Returns(Now);
        _handler = new SendChallengeHandler(_challenges, _teams, _members, _clock);
    }

    [Test]
    public async Task Handle_WhenChallengerTeamDoesNotExist_ReturnsNotFound()
    {
        _teams.GetByIdAsync(ChallengeMother.ChallengerTeamId, Arg.Any<CancellationToken>()).Returns((Team?)null);

        var result = await _handler.Handle(ChallengeMother.SendCommand(), CancellationToken.None);

        Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
        await _challenges.DidNotReceive().AddAsync(Arg.Any<Challenge>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenChallengedTeamDoesNotExist_ReturnsNotFound()
    {
        _teams.GetByIdAsync(ChallengeMother.ChallengedTeamId, Arg.Any<CancellationToken>()).Returns((Team?)null);

        var result = await _handler.Handle(ChallengeMother.SendCommand(), CancellationToken.None);

        Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
    }

    [Test]
    public async Task Handle_WhenActingUserIsNotAMember_ReturnsFailure()
    {
        _members.GetByTeamAndUserAsync(
                ChallengeMother.ChallengerTeamId, ChallengeMother.ActingUserId, Arg.Any<CancellationToken>())
            .Returns((TeamMember?)null);

        var result = await _handler.Handle(ChallengeMother.SendCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ChallengeMother.NotAllowedToSend));
        });
        await _challenges.DidNotReceive().AddAsync(Arg.Any<Challenge>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenActingUserIsAPlayer_ReturnsFailure()
    {
        _members.GetByTeamAndUserAsync(
                ChallengeMother.ChallengerTeamId, ChallengeMother.ActingUserId, Arg.Any<CancellationToken>())
            .Returns(ChallengeMother.Member(ChallengeMother.ChallengerTeamId, ChallengeMother.ActingUserId, TeamMemberRole.Player));

        var result = await _handler.Handle(ChallengeMother.SendCommand(), CancellationToken.None);

        Assert.That(result.Error, Is.EqualTo(ChallengeMother.NotAllowedToSend));
    }

    [Test]
    public async Task Handle_OnSuccess_AddsTheChallenge_AndReturnsDto()
    {
        var command = ChallengeMother.SendCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.ChallengerTeamId, Is.EqualTo(command.ChallengerTeamId));
            Assert.That(result.Value!.ChallengedTeamId, Is.EqualTo(command.ChallengedTeamId));
            Assert.That(result.Value!.Message, Is.EqualTo(command.Message));
            Assert.That(result.Value!.Status, Is.EqualTo(ChallengeStatus.Pending));
            Assert.That(result.Value!.CreatedAt, Is.EqualTo(Now));
        });
        await _challenges.Received(1).AddAsync(
            Arg.Is<Challenge>(challenge =>
                challenge.ChallengerTeamId == command.ChallengerTeamId &&
                challenge.ChallengedTeamId == command.ChallengedTeamId),
            Arg.Any<CancellationToken>());
    }
}
