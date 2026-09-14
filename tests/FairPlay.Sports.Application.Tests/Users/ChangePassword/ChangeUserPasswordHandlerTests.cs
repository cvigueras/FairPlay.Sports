using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.ChangePassword;
using FairPlay.Sports.Domain.Users;
using FairPlay.Sports.TestSupport.Users;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Users.ChangePassword;

[TestFixture]
public class ChangeUserPasswordHandlerTests
{
    private IUserRepository _users = null!;
    private IPasswordHasher _passwordHasher = null!;
    private ChangeUserPasswordHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _users = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _handler = new ChangeUserPasswordHandler(_users, _passwordHasher);
    }

    [Test]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        var command = new ChangeUserPasswordCommand(Guid.NewGuid(), "current", "new-password");
        _users.GetByIdForUpdateAsync(command.UserId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
        });
    }

    [Test]
    public async Task Handle_WhenCurrentPasswordIsWrong_ReturnsFailure_AndDoesNotChangeTheHash()
    {
        var user = UserMother.DomainUser();
        var command = new ChangeUserPasswordCommand(Guid.NewGuid(), "wrong-password", "new-password");
        _users.GetByIdForUpdateAsync(command.UserId, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(user.PasswordHash, "wrong-password").Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo("The current password is incorrect."));
            Assert.That(user.PasswordHash, Is.EqualTo(UserMother.PasswordHash));
        });
    }

    [Test]
    public async Task Handle_WhenCurrentPasswordIsCorrect_HashesAndStoresTheNewPassword()
    {
        var user = UserMother.DomainUser();
        var command = new ChangeUserPasswordCommand(Guid.NewGuid(), "current-password", "new-password");
        _users.GetByIdForUpdateAsync(command.UserId, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(user.PasswordHash, "current-password").Returns(true);
        _passwordHasher.Hash("new-password").Returns("new-hash");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(user.PasswordHash, Is.EqualTo("new-hash"));
        });
    }
}
