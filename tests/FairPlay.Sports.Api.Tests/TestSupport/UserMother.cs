using FairPlay.Sports.Api.Users;
using FairPlay.Sports.Application.Users;

namespace FairPlay.Sports.Api.Tests.TestSupport;

/// <summary>
/// Object Mother for user-related test data: every method returns a fully populated,
/// valid "archetype" with a semantic name, so tests only spell out the fields that
/// matter to the assertion at hand and there is a single place to touch when a
/// contract changes.
/// </summary>
internal static class UserMother
{
    public const string UserName = "carlos";
    public const string Email = "carlos@example.com";
    public const string Password = "Sup3rSecret!";
    public const string Team = "FairPlay FC";

    /// <summary>An active user read model, as the API returns it.</summary>
    public static UserDto Dto(Guid? id = null) =>
        new(id ?? Guid.NewGuid(), UserName, Email, Team, DateTime.UtcNow, Active: true);

    /// <summary>A valid registration request.</summary>
    public static RegisterUserRequest RegisterRequest() =>
        new(UserName, Email, Password, Team);

    /// <summary>The failure message the register handler returns when the e-mail is taken.</summary>
    public static string EmailAlreadyRegistered => $"Email '{Email}' is already registered.";
}
