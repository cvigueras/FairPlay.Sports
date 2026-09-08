using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.GetPhoto;
using FairPlay.Sports.TestSupport.Users;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Users.GetPhoto;

[TestFixture]
public class GetUserPhotoHandlerTests
{
    private IUserRepository _repository = null!;
    private GetUserPhotoHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IUserRepository>();
        _handler = new GetUserPhotoHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenPhotoExists_ReturnsIt()
    {
        var id = Guid.NewGuid();
        var photo = new UserPhoto(UserMother.PhotoBytes, UserMother.PhotoContentType);
        _repository.GetPhotoAsync(id, Arg.Any<CancellationToken>()).Returns(photo);

        var result = await _handler.Handle(new GetUserPhotoQuery(id), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.Content, Is.EqualTo(UserMother.PhotoBytes));
            Assert.That(result.Value!.ContentType, Is.EqualTo(UserMother.PhotoContentType));
        });
    }

    [Test]
    public async Task Handle_WhenNoPhoto_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetPhotoAsync(id, Arg.Any<CancellationToken>()).Returns((UserPhoto?)null);

        var result = await _handler.Handle(new GetUserPhotoQuery(id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
        });
    }
}
