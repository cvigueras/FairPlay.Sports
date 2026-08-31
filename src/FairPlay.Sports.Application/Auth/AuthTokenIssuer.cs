using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Domain.Auth;
using FairPlay.Sports.Domain.Users;

namespace FairPlay.Sports.Application.Auth;

/// <summary>
/// Shared bit of the login and refresh flows: mint an access token, mint and store a
/// rotating refresh token, and shape the <see cref="AuthResultDto"/>. It only talks to
/// driven ports, so the handlers that use it stay unit-testable. Registered in
/// <see cref="DependencyInjection"/>.
/// </summary>
public sealed class AuthTokenIssuer(
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenRepository refreshTokens,
    IClock clock) : IAuthTokenIssuer
{
    /// <summary>How long a freshly issued refresh token stays usable.</summary>
    public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(14);

    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator = refreshTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokens = refreshTokens;
    private readonly IClock _clock = clock;

    public async Task<IssuedTokens> IssueAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = _jwtTokenGenerator.Generate(user);

        var rawRefreshToken = _refreshTokenGenerator.NewToken();
        var now = _clock.UtcNow;
        var refreshToken = RefreshToken.Issue(
            Guid.NewGuid(),
            user.Id,
            _refreshTokenGenerator.Hash(rawRefreshToken),
            now,
            now.Add(RefreshTokenLifetime));

        await _refreshTokens.AddAsync(refreshToken, cancellationToken);

        var dto = new AuthResultDto(
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            rawRefreshToken,
            refreshToken.ExpiresAtUtc,
            UserDto.FromDomain(user));

        return new IssuedTokens(dto, refreshToken.Id);
    }
}

/// <param name="Result">What the handler returns to the caller.</param>
/// <param name="RefreshTokenId">Id of the new token, so a rotation can link the old one to it.</param>
public sealed record IssuedTokens(AuthResultDto Result, Guid RefreshTokenId);
