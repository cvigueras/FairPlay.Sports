using FairPlay.Sports.Application.Users;

namespace FairPlay.Sports.Api.Auth;

/// <summary>
/// Body of a successful login/refresh. The refresh token is not here - it goes back only
/// as an <c>HttpOnly</c> cookie the browser JavaScript cannot read.
/// </summary>
public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, UserDto User);
