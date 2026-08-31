using FairPlay.Sports.Domain.Users;

namespace FairPlay.Sports.Application.Auth;

/// <summary>
/// Driven port that mints a signed access token for a user. Implemented in
/// Infrastructure over <c>System.IdentityModel.Tokens.Jwt</c>.
/// </summary>
public interface IJwtTokenGenerator
{
    AccessToken Generate(User user);
}
