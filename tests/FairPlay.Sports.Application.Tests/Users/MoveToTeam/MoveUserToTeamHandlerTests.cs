using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.MoveToTeam;
using FairPlay.Sports.Domain.Users;
using FairPlay.Sports.TestSupport.Users;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Users.MoveToTeam;

[TestFixture]
public class MoveUserToTeamHandlerTests
{
    private IUserRepository _users = null!;
    private ITeamRepository _teams = null!;
    private MoveUserToTeamHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _users = Substitute.For<IUserRepository>();
        _teams = Substitute.For<ITeamRepository>();
        _teams.ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        _handler = new MoveUserToTeamHandler(_users, _teams);
    }

    [Test]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        var command = new MoveUserToTeamCommand(Guid.NewGuid(), Guid.NewGuid());
        _users.GetByIdForUpdateAsync(command.UserId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
        });
    }

    [Test]
    public async Task Handle_WhenTeamNotFound_ReturnsNotFound()
    {
        var command = new MoveUserToTeamCommand(Guid.NewGuid(), Guid.NewGuid());
        _users.GetByIdForUpdateAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(UserMother.DomainUser(id: command.UserId));
        _teams.ExistsByIdAsync(command.TeamId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
            Assert.That(result.Error, Is.EqualTo($"Team '{command.TeamId}' was not found."));
        });
    }

    [Test]
    public async Task Handle_WhenUserAndTeamExist_MovesUser_AndReturnsUpdatedDto()
    {
        var newTeamId = Guid.NewGuid();
        var command = new MoveUserToTeamCommand(Guid.NewGuid(), newTeamId);
        var user = UserMother.DomainUser(id: command.UserId);
        _users.GetByIdForUpdateAsync(command.UserId, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.TeamId, Is.EqualTo(newTeamId));
            Assert.That(user.TeamId, Is.EqualTo(newTeamId));
        });
    }
}
