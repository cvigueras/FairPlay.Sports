using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.UploadPhoto;
using FairPlay.Sports.Domain.Users;
using FairPlay.Sports.TestSupport.Users;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Users.UploadPhoto;

[TestFixture]
public class UploadUserPhotoHandlerTests
{
    private IUserRepository _repository = null!;
    private UploadUserPhotoHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IUserRepository>();
        _handler = new UploadUserPhotoHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenUserExists_SetsPhotoOnTheTrackedAggregate_AndReturnsSuccess()
    {
        var user = UserMother.DomainUser();
        _repository.GetByIdForUpdateAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var command = new UploadUserPhotoCommand(user.Id, UserMother.PhotoBytes, UserMother.PhotoContentType);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(user.HasPhoto, Is.True);
            Assert.That(user.Photo, Is.EqualTo(UserMother.PhotoBytes));
            Assert.That(user.PhotoContentType, Is.EqualTo(UserMother.PhotoContentType));
        });
    }

    [Test]
    public async Task Handle_WhenUserMissing_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdForUpdateAsync(id, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(
            new UploadUserPhotoCommand(id, UserMother.PhotoBytes, UserMother.PhotoContentType),
            CancellationToken.None);

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
        var user = UserMother.DomainUser();
        _repository.GetByIdForUpdateAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await _handler.Handle(
            new UploadUserPhotoCommand(user.Id, UserMother.PhotoBytes, UserMother.PhotoContentType),
            CancellationToken.None);

        await _repository.Received(1).GetByIdForUpdateAsync(user.Id, Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
