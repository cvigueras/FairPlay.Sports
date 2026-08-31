using FairPlay.Sports.Domain.Users;

namespace FairPlay.Sports.Application.Auth;

/// <summary>
/// Mints the access + rotating refresh token pair for an authenticated user and stores
/// the refresh token. Shared by the login and refresh handlers. Not a driven port - the
/// implementation (<see cref="AuthTokenIssuer"/>) lives in this layer and only composes
/// the real ports.
/// </summary>
public interface IAuthTokenIssuer
{
    Task<IssuedTokens> IssueAsync(User user, CancellationToken cancellationToken);
}
