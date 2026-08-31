using FairPlay.Sports.Application.Users;

namespace FairPlay.Sports.Application.Auth;

public sealed record AuthResultDto(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    UserDto User);
