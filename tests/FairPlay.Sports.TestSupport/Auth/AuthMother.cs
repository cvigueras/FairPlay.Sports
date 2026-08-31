using FairPlay.Sports.Application.Auth;
using FairPlay.Sports.Application.Auth.Login;
using FairPlay.Sports.Application.Auth.Refresh;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Domain.Auth;
using FairPlay.Sports.TestSupport.Users;

namespace FairPlay.Sports.TestSupport.Auth;

public static class AuthMother
{
    public const string RawRefreshToken = "raw-refresh-token-value";
    public const string RefreshTokenHash = "REFRESH-TOKEN-HASH";
    public const string AccessTokenValue = "signed.jwt.value";

    public const string InvalidCredentials = "Email or password is incorrect.";
    public const string InvalidRefreshToken = "The refresh token is invalid or has expired.";

    public static LoginCommand LoginCommand(string? email = null, string? password = null) =>
        new(email ?? UserMother.Email, password ?? UserMother.Password);

    public static RefreshTokenCommand RefreshCommand(string? rawToken = null) =>
        new(rawToken ?? RawRefreshToken);

    public static RefreshToken DomainRefreshToken(
        Guid? id = null,
        Guid? userId = null,
        string? tokenHash = null,
        DateTime? createdAtUtc = null,
        DateTime? expiresAtUtc = null)
    {
        var created = createdAtUtc ?? DateTime.UtcNow;
        return RefreshToken.Issue(
            id ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            tokenHash ?? RefreshTokenHash,
            created,
            expiresAtUtc ?? created.AddDays(14));
    }

    public static AccessToken AccessToken(DateTime? expiresAtUtc = null) =>
        new(AccessTokenValue, expiresAtUtc ?? DateTime.UtcNow.AddMinutes(15));

    public static AuthResultDto AuthResult(UserDto? user = null) =>
        new(
            AccessTokenValue,
            DateTime.UtcNow.AddMinutes(15),
            RawRefreshToken,
            DateTime.UtcNow.AddDays(14),
            user ?? UserMother.Dto());

    public static IssuedTokens Issued(Guid? refreshTokenId = null, AuthResultDto? result = null) =>
        new(result ?? AuthResult(), refreshTokenId ?? Guid.NewGuid());
}
