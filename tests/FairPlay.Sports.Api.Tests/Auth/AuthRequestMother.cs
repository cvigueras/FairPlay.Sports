using FairPlay.Sports.Api.Auth;
using FairPlay.Sports.TestSupport.Users;

namespace FairPlay.Sports.Api.Tests.Auth;

internal static class AuthRequestMother
{
    public static LoginRequest LoginRequest() => new(UserMother.Email, UserMother.Password);
}
