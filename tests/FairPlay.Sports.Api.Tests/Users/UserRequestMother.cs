using FairPlay.Sports.Api.Users;
using FairPlay.Sports.TestSupport.Users;

namespace FairPlay.Sports.Api.Tests.Users;

internal static class UserRequestMother
{
    public static RegisterUserRequest RegisterRequest() =>
        new(
            UserMother.UserName,
            UserMother.FirstName,
            UserMother.LastName,
            UserMother.Email,
            UserMother.Password,
            UserMother.PrimaryRole,
            AcceptedPrivacyPolicy: true,
            ConfirmedMinimumAge: true);

    public static UpdateUserProfileRequest UpdateProfileRequest() =>
        new(UserMother.UserName, UserMother.Email);

    public static ChangeUserPasswordRequest ChangePasswordRequest() =>
        new(UserMother.Password, "a-new-password");
}
