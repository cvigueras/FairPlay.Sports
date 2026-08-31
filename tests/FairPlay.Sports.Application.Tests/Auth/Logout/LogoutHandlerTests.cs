using FairPlay.Sports.Application.Auth;
using FairPlay.Sports.Application.Auth.Logout;
using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Domain.Auth;
using FairPlay.Sports.TestSupport.Auth;
using NSubstitute;

namespace FairPlay.Sports.Application.Tests.Auth.Logout;

/// <summary>
/// Unit tests for <see cref="LogoutHandler"/>: ports mocked, real aggregate used. Scope is
/// "revoke the presented token if we have it, and always succeed".
/// </summary>
[TestFixture]
public class LogoutHandlerTests
{
    private static readonly DateTime Now = new(2026, 8, 29, 12, 0, 0, DateTimeKind.Utc);

    private IRefreshTokenRepository _refreshTokens = null!;
    private IRefreshTokenGenerator _refreshTokenGenerator = null!;
    private IClock _clock = null!;
    private LogoutHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _refreshTokens = Substitute.For<IRefreshTokenRepository>();
        _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
        _clock = Substitute.For<IClock>();
        _clock.UtcNow.Returns(Now);
        _refreshTokenGenerator.Hash(AuthMother.RawRefreshToken).Returns(AuthMother.RefreshTokenHash);
        _handler = new LogoutHandler(_refreshTokens, _refreshTokenGenerator, _clock);
    }

    [Test]
    public async Task Handle_WhenTokenKnown_RevokesIt_AndSucceeds()
    {
        var stored = AuthMother.DomainRefreshToken(tokenHash: AuthMother.RefreshTokenHash);
        _refreshTokens.GetByTokenHashAsync(AuthMother.RefreshTokenHash, Arg.Any<CancellationToken>())
            .Returns(stored);

        var result = await _handler.Handle(new LogoutCommand(AuthMother.RawRefreshToken), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(stored.RevokedAtUtc, Is.EqualTo(Now));
    }

    [Test]
    public async Task Handle_WhenTokenUnknown_StillSucceeds()
    {
        _refreshTokens.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var result = await _handler.Handle(new LogoutCommand(AuthMother.RawRefreshToken), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
    }
}
