using FairPlay.Sports.Application.Auth;
using FairPlay.Sports.Application.Auth.Refresh;
using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Domain.Auth;
using FairPlay.Sports.Domain.Users;
using FairPlay.Sports.TestSupport.Auth;
using FairPlay.Sports.TestSupport.Users;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Auth.Refresh;

[TestFixture]
public class RefreshTokenHandlerTests
{
    private static readonly DateTime Now = new(2026, 8, 29, 12, 0, 0, DateTimeKind.Utc);

    private IRefreshTokenRepository _refreshTokens = null!;
    private IUserRepository _users = null!;
    private IRefreshTokenGenerator _refreshTokenGenerator = null!;
    private IAuthTokenIssuer _tokenIssuer = null!;
    private IUnitOfWork _unitOfWork = null!;
    private IClock _clock = null!;
    private RefreshTokenHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _refreshTokens = Substitute.For<IRefreshTokenRepository>();
        _users = Substitute.For<IUserRepository>();
        _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
        _tokenIssuer = Substitute.For<IAuthTokenIssuer>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _clock = Substitute.For<IClock>();
        _clock.UtcNow.Returns(Now);
        _refreshTokenGenerator.Hash(AuthMother.RawRefreshToken).Returns(AuthMother.RefreshTokenHash);
        _handler = new RefreshTokenHandler(
            _refreshTokens, _users, _refreshTokenGenerator, _tokenIssuer, _unitOfWork, _clock);
    }

    [Test]
    public async Task Handle_WhenTokenHashUnknown_ReturnsFailure()
    {
        _refreshTokens.GetByTokenHashAsync(AuthMother.RefreshTokenHash, Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var result = await _handler.Handle(AuthMother.RefreshCommand(), CancellationToken.None);

        Assert.That(result.Error, Is.EqualTo(AuthMother.InvalidRefreshToken));
        await _tokenIssuer.DidNotReceive().IssueAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenTokenExpiredButNeverRevoked_ReturnsFailure_WithoutFamilyRevocation()
    {
        var expired = AuthMother.DomainRefreshToken(
            tokenHash: AuthMother.RefreshTokenHash,
            createdAtUtc: Now.AddDays(-30),
            expiresAtUtc: Now.AddDays(-1));
        _refreshTokens.GetByTokenHashAsync(AuthMother.RefreshTokenHash, Arg.Any<CancellationToken>())
            .Returns(expired);

        var result = await _handler.Handle(AuthMother.RefreshCommand(), CancellationToken.None);

        Assert.That(result.Error, Is.EqualTo(AuthMother.InvalidRefreshToken));
        await _refreshTokens.DidNotReceive()
            .GetActiveByUserIdAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenRevokedTokenReplayed_RevokesEveryActiveTokenForTheUser_AndCommits()
    {
        var userId = Guid.NewGuid();
        var replayed = AuthMother.DomainRefreshToken(userId: userId, tokenHash: AuthMother.RefreshTokenHash);
        replayed.Revoke(Now.AddMinutes(-5));
        var otherActive = new[]
        {
            AuthMother.DomainRefreshToken(userId: userId, tokenHash: "other-1"),
            AuthMother.DomainRefreshToken(userId: userId, tokenHash: "other-2")
        };
        _refreshTokens.GetByTokenHashAsync(AuthMother.RefreshTokenHash, Arg.Any<CancellationToken>())
            .Returns(replayed);
        _refreshTokens.GetActiveByUserIdAsync(userId, Now, Arg.Any<CancellationToken>())
            .Returns(otherActive);

        var result = await _handler.Handle(AuthMother.RefreshCommand(), CancellationToken.None);

        Assert.That(result.Error, Is.EqualTo(AuthMother.InvalidRefreshToken));
        Assert.That(otherActive.Select(t => t.RevokedAtUtc), Has.All.EqualTo(Now));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _tokenIssuer.DidNotReceive().IssueAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenUserGoneOrInactive_RevokesThePresentedToken_AndCommits()
    {
        var stored = AuthMother.DomainRefreshToken(tokenHash: AuthMother.RefreshTokenHash);
        _refreshTokens.GetByTokenHashAsync(AuthMother.RefreshTokenHash, Arg.Any<CancellationToken>())
            .Returns(stored);
        _users.GetByIdAsync(stored.UserId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(AuthMother.RefreshCommand(), CancellationToken.None);

        Assert.That(result.Error, Is.EqualTo(AuthMother.InvalidRefreshToken));
        Assert.That(stored.RevokedAtUtc, Is.EqualTo(Now));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenTokenValid_RotatesTokenAndReturnsIssuerResult()
    {
        var user = UserMother.DomainUser();
        var stored = AuthMother.DomainRefreshToken(userId: user.Id, tokenHash: AuthMother.RefreshTokenHash);
        var issued = AuthMother.Issued();
        _refreshTokens.GetByTokenHashAsync(AuthMother.RefreshTokenHash, Arg.Any<CancellationToken>())
            .Returns(stored);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _tokenIssuer.IssueAsync(user, Arg.Any<CancellationToken>()).Returns(issued);

        var result = await _handler.Handle(AuthMother.RefreshCommand(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.SameAs(issued.Result));
        Assert.Multiple(() =>
        {
            Assert.That(stored.RevokedAtUtc, Is.EqualTo(Now));
            Assert.That(stored.ReplacedByTokenId, Is.EqualTo(issued.RefreshTokenId));
        });

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
