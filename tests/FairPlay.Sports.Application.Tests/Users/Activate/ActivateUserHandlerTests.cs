using FairPlay.Sports.Application.Common;
using FairPlay.Sports.TestSupport.Users;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.Activate;
using FairPlay.Sports.Domain.Users;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Users.Activate;

[TestFixture]
public class ActivateUserHandlerTests
{
    private IUserRepository _repository = null!;
    private ActivateUserHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IUserRepository>();
        _handler = new ActivateUserHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenUserExists_ActivatesTheTrackedAggregate_AndReturnsSuccess()
    {
        var user = UserMother.DomainUser(active: false);
        _repository.GetByIdForUpdateAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(new ActivateUserCommand(user.Id), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(user.Active, Is.True);
    }

    [Test]
    public async Task Handle_WhenUserAlreadyActive_IsIdempotent()
    {
        var user = UserMother.DomainUser(active: true);
        _repository.GetByIdForUpdateAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(new ActivateUserCommand(user.Id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(user.Active, Is.True);
        });
    }

    [Test]
    public async Task Handle_WhenUserHasNoTeam_ReturnsFailure_AndDoesNotActivate()
    {
        var user = UserMother.DomainUser(active: false, withTeam: false);
        _repository.GetByIdForUpdateAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(new ActivateUserCommand(user.Id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.Validation));
            Assert.That(user.Active, Is.False);
        });
    }

    [Test]
    public async Task Handle_WhenUserMissing_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdForUpdateAsync(id, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(new ActivateUserCommand(id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
            Assert.That(result.Error, Does.Contain(id.ToString()));
        });
    }

    [Test]
    public async Task Handle_LoadsTheUserThroughTheTrackedGetter()
    {
        var user = UserMother.DomainUser(active: false);
        _repository.GetByIdForUpdateAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await _handler.Handle(new ActivateUserCommand(user.Id), CancellationToken.None);

        await _repository.Received(1).GetByIdForUpdateAsync(user.Id, Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
