using FairPlay.Sports.TestSupport.Users;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.GetAll;
using FairPlay.Sports.Domain.Users;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Users.GetAll;

/// <summary>
/// Unit tests for <see cref="GetAllUsersHandler"/>: the repository port is mocked, the real
/// <c>UserDto.FromDomain</c> mapping runs. Scope is the handler's own logic - fetch, map,
/// wrap in a successful Result. Test data comes from <see cref="UserMother"/>.
/// </summary>
[TestFixture]
public class GetAllUsersHandlerTests
{
    private IUserRepository _repository = null!;
    private GetAllUsersHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IUserRepository>();
        _handler = new GetAllUsersHandler(_repository);
    }

    [Test]
    public async Task Handle_MapsEveryUserToDto()
    {
        var rivalsTeamId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var first = UserMother.DomainUser(userName: "carlos", email: "carlos@example.com");
        var second = UserMother.DomainUser(userName: "ana", email: "ana@example.com", teamId: rivalsTeamId);
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<User> { first, second });

        var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(result.Value![0].Id, Is.EqualTo(first.Id));
            Assert.That(result.Value![0].UserName, Is.EqualTo("carlos"));
            Assert.That(result.Value![0].Email, Is.EqualTo("carlos@example.com"));
            Assert.That(result.Value![0].Active, Is.True);
            Assert.That(result.Value![1].UserName, Is.EqualTo("ana"));
            Assert.That(result.Value![1].TeamId, Is.EqualTo(rivalsTeamId));
        });
    }

    [Test]
    public async Task Handle_WhenRepositoryEmpty_ReturnsSuccessWithEmptyList()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<User>());

        var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Empty);
    }

    [Test]
    public async Task Handle_ForwardsCancellationTokenToRepository()
    {
        using var cts = new CancellationTokenSource();
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<User>());

        await _handler.Handle(new GetAllUsersQuery(), cts.Token);

        await _repository.Received(1).GetAllAsync(cts.Token);
    }
}
