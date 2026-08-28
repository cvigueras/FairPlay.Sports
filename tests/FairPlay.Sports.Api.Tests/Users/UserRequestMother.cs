using FairPlay.Sports.Api.Users;
using FairPlay.Sports.TestSupport.Users;

namespace FairPlay.Sports.Api.Tests.Users;

/// <summary>
/// Api-layer companion to <see cref="UserMother"/> for shapes that live only in this project.
/// Built from the shared <see cref="UserMother"/> constants so the archetype stays in one place.
/// </summary>
internal static class UserRequestMother
{
    /// <summary>A valid registration request.</summary>
    public static RegisterUserRequest RegisterRequest() =>
        new(UserMother.UserName, UserMother.Email, UserMother.Password, UserMother.Team);
}
