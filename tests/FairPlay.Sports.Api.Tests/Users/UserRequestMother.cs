using FairPlay.Sports.Api.Users;
using FairPlay.Sports.TestSupport.Users;

namespace FairPlay.Sports.Api.Tests.Users;

internal static class UserRequestMother
{
    public static RegisterUserRequest RegisterRequest() =>
        new(UserMother.UserName, UserMother.Email, UserMother.Password, UserMother.TeamId);
}
