using FairPlay.Sports.Application.Auth;
using FairPlay.Sports.Application.Auth.Login;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Domain.Users;
using FairPlay.Sports.TestSupport.Auth;
using FairPlay.Sports.TestSupport.Users;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Auth.Login;


[TestFixture]
public class LoginHandlerTests
{
    private IUserRepository _users = null!;
    private IPasswordHasher _passwordHasher = null!;
    private IAuthTokenIssuer _tokenIssuer = null!;
    private LoginHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _users = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenIssuer = Substitute.For<IAuthTokenIssuer>();
        _handler = new LoginHandler(_users, _passwordHasher, _tokenIssuer);
    }

    [Test]
    public async Task Handle_WhenEmailUnknown_ReturnsGenericFailure_AndDoesNotIssueTokens()
    {
        _users.GetByEmailAsync(UserMother.Email, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(AuthMother.LoginCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(AuthMother.InvalidCredentials));
        });
        await _tokenIssuer.DidNotReceive().IssueAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenPasswordDoesNotVerify_ReturnsGenericFailure()
    {
        var user = UserMother.DomainUser();
        _users.GetByEmailAsync(UserMother.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(user.PasswordHash, UserMother.Password).Returns(false);

        var result = await _handler.Handle(AuthMother.LoginCommand(), CancellationToken.None);

        Assert.That(result.Error, Is.EqualTo(AuthMother.InvalidCredentials));
        await _tokenIssuer.DidNotReceive().IssueAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenUserInactive_StillSignsIn()
    {
        // Newly registered users are inactive until they have a team; that gates
        // team features, not sign-in.
        var user = UserMother.DomainUser(active: false);
        var issued = AuthMother.Issued();
        _users.GetByEmailAsync(UserMother.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(user.PasswordHash, UserMother.Password).Returns(true);
        _tokenIssuer.IssueAsync(user, Arg.Any<CancellationToken>()).Returns(issued);

        var result = await _handler.Handle(AuthMother.LoginCommand(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.SameAs(issued.Result));
    }

    [Test]
    public async Task Handle_WhenCredentialsValid_ReturnsIssuerResult()
    {
        var user = UserMother.DomainUser();
        var issued = AuthMother.Issued();
        _users.GetByEmailAsync(UserMother.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(user.PasswordHash, UserMother.Password).Returns(true);
        _tokenIssuer.IssueAsync(user, Arg.Any<CancellationToken>()).Returns(issued);

        var result = await _handler.Handle(AuthMother.LoginCommand(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.SameAs(issued.Result));
        await _tokenIssuer.Received(1).IssueAsync(user, Arg.Any<CancellationToken>());
    }
}
