using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.SetVenueAvailable;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.TestSupport.Challenges;
using FairPlay.Sports.TestSupport.Teams;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Teams.SetVenueAvailable;

[TestFixture]
public class SetVenueAvailableHandlerTests
{
    private static readonly Guid ActingUserId = Guid.NewGuid();

    private ITeamRepository _teams = null!;
    private ITeamMemberRepository _members = null!;
    private SetVenueAvailableHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _teams = Substitute.For<ITeamRepository>();
        _members = Substitute.For<ITeamMemberRepository>();
        _handler = new SetVenueAvailableHandler(_teams, _members);
    }

    private Team ExistingTeam()
    {
        var team = TeamMother.DomainTeam();
        _teams.GetByIdForUpdateAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);
        return team;
    }

    private void ActingUserHasRole(Team team, TeamMemberRole role) =>
        _members.GetByTeamAndUserAsync(team.Id, ActingUserId, Arg.Any<CancellationToken>())
            .Returns(ChallengeMother.Member(team.Id, ActingUserId, role));

    [TestCase(TeamMemberRole.Delegate)]
    [TestCase(TeamMemberRole.Coach)]
    [TestCase(TeamMemberRole.President)]
    [TestCase(TeamMemberRole.TechnicalStaff)]
    public async Task Handle_WhenActingUserCanActForTheTeam_SetsTheFlagOnTheTrackedTeam(TeamMemberRole role)
    {
        var team = ExistingTeam();
        ActingUserHasRole(team, role);

        var result = await _handler.Handle(
            new SetVenueAvailableCommand(team.Id, true, ActingUserId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.VenueAvailable, Is.True);
            Assert.That(team.VenueAvailable, Is.True);
        });
    }

    [Test]
    public async Task Handle_CanTurnTheFlagBackOff()
    {
        var team = ExistingTeam();
        team.SetVenueAvailable(true);
        ActingUserHasRole(team, TeamMemberRole.Coach);

        var result = await _handler.Handle(
            new SetVenueAvailableCommand(team.Id, false, ActingUserId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(team.VenueAvailable, Is.False);
        });
    }

    [Test]
    public async Task Handle_WhenActingUserIsOnlyAPlayer_ReturnsFailure_AndLeavesTheFlag()
    {
        var team = ExistingTeam();
        ActingUserHasRole(team, TeamMemberRole.Player);

        var result = await _handler.Handle(
            new SetVenueAvailableCommand(team.Id, true, ActingUserId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.Validation));
            Assert.That(team.VenueAvailable, Is.False);
        });
    }

    [Test]
    public async Task Handle_WhenActingUserIsNotAMember_ReturnsFailure_AndLeavesTheFlag()
    {
        var team = ExistingTeam();
        _members.GetByTeamAndUserAsync(team.Id, ActingUserId, Arg.Any<CancellationToken>())
            .Returns((TeamMember?)null);

        var result = await _handler.Handle(
            new SetVenueAvailableCommand(team.Id, true, ActingUserId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(team.VenueAvailable, Is.False);
        });
    }

    [Test]
    public async Task Handle_WhenTeamMissing_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _teams.GetByIdForUpdateAsync(id, Arg.Any<CancellationToken>()).Returns((Team?)null);

        var result = await _handler.Handle(
            new SetVenueAvailableCommand(id, true, ActingUserId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
        });
    }
}
