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
    private static readonly KitColors GreenKit = new("Green", "White", "White", KitPattern.Plain);
    private static readonly KitColors BlueKit = new("Blue", "Yellow", "Blue", KitPattern.Plain);
    private static readonly KitColors RedKit = new("Red", "Black", "Black", KitPattern.Plain);
    private static readonly KitColors PurpleKit = new("Purple", "White", "Purple", KitPattern.Plain);

    private IChallengeRepository _challenges = null!;
    private ITeamRepository _teams = null!;
    private ITeamMemberRepository _members = null!;
    private IClock _clock = null!;
    private SendChallengeHandler _handler = null!;
    private Team _challengerTeam = null!;
    private Team _challengedTeam = null!;

    private static Team TeamWithProfile(Guid id, KitColors colors, KitColors? alternateColors = null) =>
        TeamMother.DomainTeam(id: id, profile: new TeamProfile(Colors: colors, AlternateColors: alternateColors));

    [SetUp]
    public void SetUp()
    {
        _challengerTeam = TeamWithProfile(ChallengeMother.ChallengerTeamId, GreenKit);
        _challengedTeam = TeamWithProfile(ChallengeMother.ChallengedTeamId, BlueKit, RedKit);

        _challenges = Substitute.For<IChallengeRepository>();
        _teams = Substitute.For<ITeamRepository>();
        _teams.GetByIdAsync(ChallengeMother.ChallengerTeamId, Arg.Any<CancellationToken>()).Returns(_challengerTeam);
        _teams.GetByIdAsync(ChallengeMother.ChallengedTeamId, Arg.Any<CancellationToken>()).Returns(_challengedTeam);
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
    public async Task Handle_WhenActingUserIsTechnicalStaff_Succeeds()
    {
        _members.GetByTeamAndUserAsync(
                ChallengeMother.ChallengerTeamId, ChallengeMother.ActingUserId, Arg.Any<CancellationToken>())
            .Returns(ChallengeMother.Member(ChallengeMother.ChallengerTeamId, ChallengeMother.ActingUserId, TeamMemberRole.TechnicalStaff));

        var result = await _handler.Handle(ChallengeMother.SendCommand(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task Handle_WhenMatchDateIsInThePast_ReturnsFailure()
    {
        var result = await _handler.Handle(
            ChallengeMother.SendCommand(matchDate: Now.AddDays(-1)), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ChallengeMother.MatchDateNotInFuture));
        });
    }

    [Test]
    public async Task Handle_WhenHomeTeamHasNoKitConfigured_Succeeds_WithNoHomeKit_AndAwayUsesItsOwnFirstKit()
    {
        _challengerTeam = TeamMother.DomainTeam(id: ChallengeMother.ChallengerTeamId);
        _teams.GetByIdAsync(ChallengeMother.ChallengerTeamId, Arg.Any<CancellationToken>()).Returns(_challengerTeam);

        var result = await _handler.Handle(
            ChallengeMother.SendCommand(venueTeamId: ChallengeMother.ChallengerTeamId), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.HomeKit, Is.Null);
            Assert.That(result.Value!.AwayKit?.ColorPrimary, Is.EqualTo(BlueKit.Primary));
            Assert.That(result.Value!.KitsClash, Is.False);
        });
    }

    [Test]
    public async Task Handle_WhenAwayTeamHasNoKitAtAll_Succeeds_WithNoAwayKit()
    {
        _challengedTeam = TeamMother.DomainTeam(id: ChallengeMother.ChallengedTeamId);
        _teams.GetByIdAsync(ChallengeMother.ChallengedTeamId, Arg.Any<CancellationToken>()).Returns(_challengedTeam);

        var result = await _handler.Handle(
            ChallengeMother.SendCommand(venueTeamId: ChallengeMother.ChallengerTeamId), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.HomeKit?.ColorPrimary, Is.EqualTo(GreenKit.Primary));
            Assert.That(result.Value!.AwayKit, Is.Null);
            Assert.That(result.Value!.KitsClash, Is.False);
        });
    }

    [Test]
    public async Task Handle_WhenVenueIsChallenger_AndAwayFirstKitDoesNotClash_UsesFirstKit()
    {
        var result = await _handler.Handle(
            ChallengeMother.SendCommand(venueTeamId: ChallengeMother.ChallengerTeamId), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.HomeTeamId, Is.EqualTo(ChallengeMother.ChallengerTeamId));
            Assert.That(result.Value!.AwayTeamId, Is.EqualTo(ChallengeMother.ChallengedTeamId));
            Assert.That(result.Value!.AwayKit!.Slot, Is.EqualTo(TeamKitSlot.First));
            Assert.That(result.Value!.AwayKit!.ColorPrimary, Is.EqualTo(BlueKit.Primary));
            Assert.That(result.Value!.HomeKit!.ColorPrimary, Is.EqualTo(GreenKit.Primary));
        });
    }

    [Test]
    public async Task Handle_WhenAwayFirstKitClashesWithHome_FallsBackToSecondKit()
    {
        // Challenged plays away this time, with a first kit that matches the home (challenger) colour.
        _challengedTeam = TeamWithProfile(ChallengeMother.ChallengedTeamId, GreenKit, RedKit);
        _teams.GetByIdAsync(ChallengeMother.ChallengedTeamId, Arg.Any<CancellationToken>()).Returns(_challengedTeam);
        _members.GetByTeamAndUserAsync(
                ChallengeMother.ChallengerTeamId, ChallengeMother.ActingUserId, Arg.Any<CancellationToken>())
            .Returns(ChallengeMother.Member(ChallengeMother.ChallengerTeamId, ChallengeMother.ActingUserId, TeamMemberRole.Delegate));

        var result = await _handler.Handle(
            ChallengeMother.SendCommand(venueTeamId: ChallengeMother.ChallengerTeamId), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.AwayKit!.Slot, Is.EqualTo(TeamKitSlot.Second));
            Assert.That(result.Value!.AwayKit!.ColorPrimary, Is.EqualTo(RedKit.Primary));
        });
    }

    [Test]
    public async Task Handle_WhenBothAwayKitsClashWithHome_Succeeds_UsingFirstKitAnyway_AndFlagsTheClash()
    {
        _challengedTeam = TeamWithProfile(ChallengeMother.ChallengedTeamId, GreenKit, GreenKit);
        _teams.GetByIdAsync(ChallengeMother.ChallengedTeamId, Arg.Any<CancellationToken>()).Returns(_challengedTeam);

        var result = await _handler.Handle(
            ChallengeMother.SendCommand(venueTeamId: ChallengeMother.ChallengerTeamId), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.AwayKit!.Slot, Is.EqualTo(TeamKitSlot.First));
            Assert.That(result.Value!.AwayKit!.ColorPrimary, Is.EqualTo(GreenKit.Primary));
            Assert.That(result.Value!.KitsClash, Is.True);
        });
        await _challenges.Received(1).AddAsync(Arg.Any<Challenge>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenAwayHasNoSecondKit_AndFirstClashes_Succeeds_AndFlagsTheClash()
    {
        _challengedTeam = TeamWithProfile(ChallengeMother.ChallengedTeamId, GreenKit);
        _teams.GetByIdAsync(ChallengeMother.ChallengedTeamId, Arg.Any<CancellationToken>()).Returns(_challengedTeam);

        var result = await _handler.Handle(
            ChallengeMother.SendCommand(venueTeamId: ChallengeMother.ChallengerTeamId), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.AwayKit!.Slot, Is.EqualTo(TeamKitSlot.First));
            Assert.That(result.Value!.KitsClash, Is.True);
        });
    }

    [Test]
    public async Task Handle_WhenChallengerIsHome_AndPrefersItsSecondKit_AndHasIt_UsesThePreferenceOverTheDefault()
    {
        _challengerTeam = TeamWithProfile(ChallengeMother.ChallengerTeamId, GreenKit, PurpleKit);
        _teams.GetByIdAsync(ChallengeMother.ChallengerTeamId, Arg.Any<CancellationToken>()).Returns(_challengerTeam);

        var result = await _handler.Handle(
            ChallengeMother.SendCommand(
                venueTeamId: ChallengeMother.ChallengerTeamId,
                challengerKitPreference: TeamKitSlot.Second),
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.HomeTeamId, Is.EqualTo(ChallengeMother.ChallengerTeamId));
            Assert.That(result.Value!.HomeKit!.Slot, Is.EqualTo(TeamKitSlot.Second));
            Assert.That(result.Value!.HomeKit!.ColorPrimary, Is.EqualTo(PurpleKit.Primary));
            // The away side still resolves automatically, now against the chosen home kit.
            Assert.That(result.Value!.AwayKit!.Slot, Is.EqualTo(TeamKitSlot.First));
            Assert.That(result.Value!.AwayKit!.ColorPrimary, Is.EqualTo(BlueKit.Primary));
        });
    }

    [Test]
    public async Task Handle_WhenChallengerIsHome_AndPrefersAKitItDoesNotHave_FallsBackToTheDefault()
    {
        // Challenger only has a first kit.
        var result = await _handler.Handle(
            ChallengeMother.SendCommand(
                venueTeamId: ChallengeMother.ChallengerTeamId,
                challengerKitPreference: TeamKitSlot.Second),
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.HomeKit!.Slot, Is.EqualTo(TeamKitSlot.First));
            Assert.That(result.Value!.HomeKit!.ColorPrimary, Is.EqualTo(GreenKit.Primary));
        });
    }

    [Test]
    public async Task Handle_WhenChallengerIsAway_AndPrefersItsSecondKit_AndHasIt_UsesThePreferenceOverTheDefault()
    {
        _challengerTeam = TeamWithProfile(ChallengeMother.ChallengerTeamId, GreenKit, PurpleKit);
        _teams.GetByIdAsync(ChallengeMother.ChallengerTeamId, Arg.Any<CancellationToken>()).Returns(_challengerTeam);

        var result = await _handler.Handle(
            ChallengeMother.SendCommand(
                venueTeamId: ChallengeMother.ChallengedTeamId,
                challengerKitPreference: TeamKitSlot.Second),
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.HomeTeamId, Is.EqualTo(ChallengeMother.ChallengedTeamId));
            Assert.That(result.Value!.AwayKit!.Slot, Is.EqualTo(TeamKitSlot.Second));
            Assert.That(result.Value!.AwayKit!.ColorPrimary, Is.EqualTo(PurpleKit.Primary));
            // The home side still resolves automatically, now against the chosen away kit.
            Assert.That(result.Value!.HomeKit!.Slot, Is.EqualTo(TeamKitSlot.First));
            Assert.That(result.Value!.HomeKit!.ColorPrimary, Is.EqualTo(BlueKit.Primary));
        });
    }

    [Test]
    public async Task Handle_WhenTheChosenAwayKitClashesWithHome_HomeFallsBackToItsOwnSecondKit()
    {
        // Away's preferred second kit (Blue) clashes with the home team's first kit (also Blue) -
        // the home side's automatic resolution reacts to whichever kit away actually wears, not
        // always its first.
        _challengerTeam = TeamWithProfile(ChallengeMother.ChallengerTeamId, GreenKit, BlueKit);
        _teams.GetByIdAsync(ChallengeMother.ChallengerTeamId, Arg.Any<CancellationToken>()).Returns(_challengerTeam);

        var result = await _handler.Handle(
            ChallengeMother.SendCommand(
                venueTeamId: ChallengeMother.ChallengedTeamId,
                challengerKitPreference: TeamKitSlot.Second),
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.AwayKit!.ColorPrimary, Is.EqualTo(BlueKit.Primary));
            Assert.That(result.Value!.HomeKit!.Slot, Is.EqualTo(TeamKitSlot.Second));
            Assert.That(result.Value!.HomeKit!.ColorPrimary, Is.EqualTo(RedKit.Primary));
        });
    }

    [Test]
    public async Task Handle_WhenTheChosenHomeKitClashesWithAway_AwayFallsBackToItsOwnSecondKit()
    {
        // Home's preferred second kit (Blue) clashes with the away team's first kit (also Blue) -
        // the away side's automatic resolution reacts to whichever kit home actually wears, not
        // always its first.
        _challengerTeam = TeamWithProfile(ChallengeMother.ChallengerTeamId, GreenKit, BlueKit);
        _teams.GetByIdAsync(ChallengeMother.ChallengerTeamId, Arg.Any<CancellationToken>()).Returns(_challengerTeam);

        var result = await _handler.Handle(
            ChallengeMother.SendCommand(
                venueTeamId: ChallengeMother.ChallengerTeamId,
                challengerKitPreference: TeamKitSlot.Second),
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.HomeKit!.ColorPrimary, Is.EqualTo(BlueKit.Primary));
            Assert.That(result.Value!.AwayKit!.Slot, Is.EqualTo(TeamKitSlot.Second));
            Assert.That(result.Value!.AwayKit!.ColorPrimary, Is.EqualTo(RedKit.Primary));
        });
    }

    [Test]
    public async Task Handle_OnSuccess_AddsTheChallenge_AndReturnsDto()
    {
        var command = ChallengeMother.SendCommand(venueTeamId: ChallengeMother.ChallengerTeamId);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.ChallengerTeamId, Is.EqualTo(command.ChallengerTeamId));
            Assert.That(result.Value!.ChallengedTeamId, Is.EqualTo(command.ChallengedTeamId));
            Assert.That(result.Value!.Message, Is.EqualTo(command.Message));
            Assert.That(result.Value!.MatchDate, Is.EqualTo(command.MatchDate));
            Assert.That(result.Value!.Status, Is.EqualTo(ChallengeStatus.Pending));
            Assert.That(result.Value!.CreatedAt, Is.EqualTo(Now));
        });
        await _challenges.Received(1).AddAsync(
            Arg.Is<Challenge>(challenge =>
                challenge.ChallengerTeamId == command.ChallengerTeamId &&
                challenge.ChallengedTeamId == command.ChallengedTeamId &&
                challenge.VenueTeamId == command.VenueTeamId &&
                challenge.MatchDate == command.MatchDate),
            Arg.Any<CancellationToken>());
    }
}
