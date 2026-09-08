using FairPlay.Sports.Application.Common;
using FairPlay.Sports.TestSupport.Users;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.Register;
using FairPlay.Sports.Domain.Users;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Users.Register;

/// <summary>
/// Unit tests for <see cref="RegisterUserHandler"/>: both ports (<see cref="IUserRepository"/>
/// and <see cref="IPasswordHasher"/>) are mocked, the real <see cref="User"/> aggregate is
/// built. Scope is the handler's own logic - the uniqueness guards, hashing the clear-text
/// password before it is persisted, and building the created-user DTO. FluentValidation and
/// the unit-of-work commit are pipeline behaviors and out of scope here. Test data comes from
/// <see cref="UserMother"/>.
/// </summary>
[TestFixture]
public class RegisterUserHandlerTests
{
    private static readonly DateTime Now = new(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc);

    private IUserRepository _repository = null!;
    private ITeamRepository _teams = null!;
    private IPasswordHasher _passwordHasher = null!;
    private IClock _clock = null!;
    private RegisterUserHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IUserRepository>();
        _teams = Substitute.For<ITeamRepository>();
        _teams.ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _clock = Substitute.For<IClock>();
        _clock.UtcNow.Returns(Now);
        _handler = new RegisterUserHandler(_repository, _teams, _passwordHasher, _clock);
    }

    [Test]
    public async Task Handle_WhenEmailTaken_ReturnsFailure_AndDoesNotPersistOrHash()
    {
        _repository.ExistsByEmailAsync(UserMother.Email, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(UserMother.Command(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.Validation));
            Assert.That(result.Error, Is.EqualTo(UserMother.EmailAlreadyRegistered));
        });
        await _repository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        _passwordHasher.DidNotReceive().Hash(Arg.Any<string>());
    }

    [Test]
    public async Task Handle_WhenUserNameTaken_ReturnsFailure_AndDoesNotPersist()
    {
        _repository.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _repository.ExistsByUserNameAsync(UserMother.UserName, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(UserMother.Command(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(UserMother.UserNameAlreadyTaken));
        });
        await _repository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenEmailAndUserNameBothTaken_ReportsEmailFirst()
    {
        _repository.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        _repository.ExistsByUserNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(UserMother.Command(), CancellationToken.None);

        Assert.That(result.Error, Is.EqualTo(UserMother.EmailAlreadyRegistered));
    }

    [Test]
    public async Task Handle_WhenTeamDoesNotExist_ReturnsNotFound_AndDoesNotPersist()
    {
        _repository.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _repository.ExistsByUserNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _teams.ExistsByIdAsync(UserMother.TeamId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(UserMother.Command(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorType, Is.EqualTo(ResultErrorType.NotFound));
            Assert.That(result.Error, Is.EqualTo($"Team '{UserMother.TeamId}' was not found."));
        });
        await _repository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        _passwordHasher.DidNotReceive().Hash(Arg.Any<string>());
    }

    [Test]
    public async Task Handle_WithoutTeam_RegistersUserWithNoTeam_AndSkipsTheTeamLookup()
    {
        _repository.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _repository.ExistsByUserNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _passwordHasher.Hash(UserMother.Password).Returns("HASHED");

        var result = await _handler.Handle(UserMother.Command() with { TeamId = null }, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.TeamId, Is.Null);
            Assert.That(result.Value!.Active, Is.False);
        });
        await _teams.DidNotReceive().ExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).AddAsync(
            Arg.Is<User>(user => user.TeamId == null && !user.Active), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenRegisteredWithAKnownTeam_HashesPassword_PersistsActiveUser_AndReturnsDto()
    {
        _repository.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _repository.ExistsByUserNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _passwordHasher.Hash(UserMother.Password).Returns("HASHED");

        var result = await _handler.Handle(UserMother.Command(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value!.UserName, Is.EqualTo(UserMother.UserName));
            Assert.That(result.Value!.Email, Is.EqualTo(UserMother.Email));
            Assert.That(result.Value!.TeamId, Is.EqualTo(UserMother.TeamId));
            Assert.That(result.Value!.Active, Is.True);
            Assert.That(result.Value!.CreatedAt, Is.EqualTo(Now));
            Assert.That(result.Value!.Id, Is.Not.EqualTo(Guid.Empty));
        });

        await _repository.Received(1).AddAsync(
            Arg.Is<User>(user =>
                user.PasswordHash == "HASHED" &&
                user.UserName == UserMother.UserName &&
                user.Email == UserMother.Email &&
                user.TeamId == UserMother.TeamId &&
                user.Active &&
                user.Id != Guid.Empty &&
                user.CreatedAt == Now),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_HashesTheClearTextPasswordTakenFromTheCommand()
    {
        _repository.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _repository.ExistsByUserNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _passwordHasher.Hash(Arg.Any<string>()).Returns("HASHED");

        await _handler.Handle(UserMother.Command(), CancellationToken.None);

        _passwordHasher.Received(1).Hash(UserMother.Password);
    }
}
