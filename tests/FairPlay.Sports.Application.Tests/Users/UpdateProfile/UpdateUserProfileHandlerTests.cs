using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.UpdateProfile;
using FairPlay.Sports.Domain.Users;
using FairPlay.Sports.TestSupport.Users;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Users.UpdateProfile;

[TestFixture]
public class UpdateUserProfileHandlerTests
{
    private IUserRepository _users = null!;
    private UpdateUserProfileHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _users = Substitute.For<IUserRepository>();
        _handler = new UpdateUserProfileHandler(_users);
    }

    [Test]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        var command = new UpdateUserProfileCommand(Guid.NewGuid(), "new-name", "new@example.com");
        _users.GetByIdForUpdateAsync(command.UserId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
        });
    }

    [Test]
    public async Task Handle_WhenUserNameChangedToOneAlreadyTaken_ReturnsFailure()
    {
        var user = UserMother.DomainUser();
        var command = new UpdateUserProfileCommand(Guid.NewGuid(), "someone-else", user.Email);
        _users.GetByIdForUpdateAsync(command.UserId, Arg.Any<CancellationToken>()).Returns(user);
        _users.ExistsByUserNameAsync("someone-else", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo("User name 'someone-else' is already taken."));
        });
    }

    [Test]
    public async Task Handle_WhenEmailChangedToOneAlreadyRegistered_ReturnsFailure()
    {
        var user = UserMother.DomainUser();
        var command = new UpdateUserProfileCommand(Guid.NewGuid(), user.UserName, "taken@example.com");
        _users.GetByIdForUpdateAsync(command.UserId, Arg.Any<CancellationToken>()).Returns(user);
        _users.ExistsByEmailAsync("taken@example.com", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo("Email 'taken@example.com' is already registered."));
        });
    }

    [Test]
    public async Task Handle_WhenUserNameAndEmailAreUnchanged_DoesNotCheckUniqueness_AndSucceeds()
    {
        var user = UserMother.DomainUser();
        var command = new UpdateUserProfileCommand(Guid.NewGuid(), user.UserName, user.Email);
        _users.GetByIdForUpdateAsync(command.UserId, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        await _users.DidNotReceive().ExistsByUserNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _users.DidNotReceive().ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenValid_UpdatesTheUser_AndReturnsUpdatedDto()
    {
        var user = UserMother.DomainUser();
        var command = new UpdateUserProfileCommand(Guid.NewGuid(), "new-name", "new@example.com");
        _users.GetByIdForUpdateAsync(command.UserId, Arg.Any<CancellationToken>()).Returns(user);
        _users.ExistsByUserNameAsync("new-name", Arg.Any<CancellationToken>()).Returns(false);
        _users.ExistsByEmailAsync("new@example.com", Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.UserName, Is.EqualTo("new-name"));
            Assert.That(result.Value!.Email, Is.EqualTo("new@example.com"));
            Assert.That(user.UserName, Is.EqualTo("new-name"));
            Assert.That(user.Email, Is.EqualTo("new@example.com"));
        });
    }
}
