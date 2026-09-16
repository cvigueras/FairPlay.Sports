using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.JoinTeam;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.Domain.Users;
using FairPlay.Sports.TestSupport.Teams;
using FairPlay.Sports.TestSupport.Users;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Teams.JoinTeam;

/// <summary>
/// Unit tests for <see cref="JoinTeamHandler"/>: all ports mocked, the real
/// <see cref="TeamMember"/> aggregate is built. Scope is the handler's own logic - team/user
/// existence, the one-role-per-team guard, the at-most-one-non-player-role-per-team guard, and
/// activating the user on join. Test data comes from <see cref="TeamMemberMother"/>.
/// </summary>
[TestFixture]
public class JoinTeamHandlerTests
{
    private static readonly DateTime Now = new(2026, 9, 16, 12, 0, 0, DateTimeKind.Utc);

    private ITeamRepository _teams = null!;
    private IUserRepository _users = null!;
    private ITeamMemberRepository _members = null!;
    private IClock _clock = null!;
    private JoinTeamHandler _handler = null!;
    private User _user = null!;

    [SetUp]
    public void SetUp()
    {
        _teams = Substitute.For<ITeamRepository>();
        _teams.ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        _users = Substitute.For<IUserRepository>();
        _user = UserMother.DomainUser(id: TeamMemberMother.UserId, active: false);
        _users.GetByIdForUpdateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(_user);
        _members = Substitute.For<ITeamMemberRepository>();
        _members.ExistsForUserAndTeamAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _members.ExistsWithRoleAsync(Arg.Any<Guid>(), Arg.Any<TeamMemberRole>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _clock = Substitute.For<IClock>();
        _clock.UtcNow.Returns(Now);
        _handler = new JoinTeamHandler(_teams, _users, _members, _clock);
    }

    [Test]
    public async Task Handle_WhenTeamDoesNotExist_ReturnsNotFound()
    {
        _teams.ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(TeamMemberMother.JoinCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
        });
        await _members.DidNotReceive().AddAsync(Arg.Any<TeamMember>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenUserDoesNotExist_ReturnsNotFound()
    {
        _users.GetByIdForUpdateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(TeamMemberMother.JoinCommand(), CancellationToken.None);

        Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
        await _members.DidNotReceive().AddAsync(Arg.Any<TeamMember>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenUserAlreadyBelongsToTeam_ReturnsFailure()
    {
        _members.ExistsForUserAndTeamAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(TeamMemberMother.JoinCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.Validation));
            Assert.That(result.Error, Is.EqualTo(TeamMemberMother.AlreadyMember));
        });
        await _members.DidNotReceive().AddAsync(Arg.Any<TeamMember>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenNonPlayerRoleAlreadyTaken_ReturnsFailure()
    {
        _members.ExistsWithRoleAsync(Arg.Any<Guid>(), TeamMemberRole.Coach, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(
            TeamMemberMother.JoinCommand(role: TeamMemberRole.Coach), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(TeamMemberMother.RoleAlreadyTaken(TeamMemberRole.Coach)));
        });
        await _members.DidNotReceive().AddAsync(Arg.Any<TeamMember>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenSecondPlayerJoins_Succeeds_WithoutCheckingRoleUniqueness()
    {
        var result = await _handler.Handle(
            TeamMemberMother.JoinCommand(role: TeamMemberRole.Player), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        await _members.DidNotReceive().ExistsWithRoleAsync(
            Arg.Any<Guid>(), Arg.Any<TeamMemberRole>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_OnSuccess_AddsTheMember_ActivatesTheUser_AndReturnsDto()
    {
        var command = TeamMemberMother.JoinCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.TeamId, Is.EqualTo(command.TeamId));
            Assert.That(result.Value!.UserId, Is.EqualTo(command.UserId));
            Assert.That(result.Value!.Role, Is.EqualTo(command.Role));
            Assert.That(result.Value!.DisplayName, Is.EqualTo(command.DisplayName));
            Assert.That(result.Value!.CreatedAt, Is.EqualTo(Now));
            Assert.That(_user.Active, Is.True);
        });
        await _members.Received(1).AddAsync(
            Arg.Is<TeamMember>(member =>
                member.TeamId == command.TeamId &&
                member.UserId == command.UserId &&
                member.Role == command.Role),
            Arg.Any<CancellationToken>());
    }
}
