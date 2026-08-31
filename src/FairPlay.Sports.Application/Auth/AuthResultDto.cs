using FairPlay.Sports.Application.Users;

namespace FairPlay.Sports.Application.Auth;

/// <summary>
/// Outcome of a successful login or refresh. Carries the raw refresh token so the Api
/// layer can drop it into an <c>HttpOnly</c> cookie; it is never serialised straight to
/// the client.
/// </summary>
public sealed record AuthResultDto(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    UserDto User);
