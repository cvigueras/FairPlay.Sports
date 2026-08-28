using FairPlay.Sports.Application.Common;
using FairPlay.Sports.TestSupport.Users;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.GetById;
using FairPlay.Sports.Domain.Users;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Users.GetById;

/// <summary>
/// Unit tests for <see cref="GetUserByIdHandler"/>: the repository port is mocked, the real
/// <c>UserDto.FromDomain</c> mapping runs. Scope is the handler's own logic - look up by id and
/// translate "missing" into a NotFound Result. Test data comes from <see cref="UserMother"/>.
/// </summary>
[TestFixture]
public class GetUserByIdHandlerTests
{
    private IUserRepository _repository = null!;
    private GetUserByIdHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IUserRepository>();
        _handler = new GetUserByIdHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenUserExists_ReturnsSuccessWithMappedDto()
    {
        var user = UserMother.DomainUser();
        _repository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.Id, Is.EqualTo(user.Id));
            Assert.That(result.Value!.UserName, Is.EqualTo(user.UserName));
            Assert.That(result.Value!.Email, Is.EqualTo(user.Email));
            Assert.That(result.Value!.Team, Is.EqualTo(user.Team));
            Assert.That(result.Value!.Active, Is.EqualTo(user.Active));
        });
    }

    [Test]
    public async Task Handle_WhenUserMissing_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(new GetUserByIdQuery(id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
            Assert.That(result.Error, Does.Contain(id.ToString()));
        });
    }

    [Test]
    public async Task Handle_QueriesRepositoryWithRequestedId()
    {
        var id = Guid.NewGuid();

        await _handler.Handle(new GetUserByIdQuery(id), CancellationToken.None);

        await _repository.Received(1).GetByIdAsync(id, Arg.Any<CancellationToken>());
    }
}
