using FairPlay.Sports.Api.Users;
using FairPlay.Sports.TestSupport.Users;

namespace FairPlay.Sports.Api.Tests.Users;

internal static class UserRequestMother
{
    public static RegisterUserRequest RegisterRequest() =>
        new(UserMother.UserName, UserMother.Email, UserMother.Password);

    public static UpdateUserProfileRequest UpdateProfileRequest() =>
        new(UserMother.UserName, UserMother.Email);

    public static ChangeUserPasswordRequest ChangePasswordRequest() =>
        new(UserMother.Password, "a-new-password");
}
