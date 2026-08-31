using FairPlay.Sports.Application.Users;

namespace FairPlay.Sports.Api.Auth;

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, UserDto User);
