using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.Register;
using FairPlay.Sports.Domain.Users;

namespace FairPlay.Sports.TestSupport.Users;

/// <summary>
/// Object Mother for user-related test data, shared by every test project: each member returns a
/// fully populated, valid "archetype" with a semantic name, so tests only spell out the fields
/// that matter to the assertion at hand and there is a single place to touch when a contract
/// changes. Layer-specific shapes this project cannot see (e.g. the Api request) are built from
/// these same constants by a small companion mother in that test project.
/// </summary>
public static class UserMother
{
    public const string UserName = "carlos";
    public const string Email = "carlos@example.com";
    public const string Password = "not-a-real-password";
    public const string PasswordHash = "hashed-password";
    public const string Team = "FairPlay FC";

    /// <summary>A valid registration command.</summary>
    public static RegisterUserCommand Command() => new(UserName, Email, Password, Team);

    /// <summary>An active domain user in its persisted shape (holds a hash, never clear text).</summary>
    public static User DomainUser(
        Guid? id = null,
        string? userName = null,
        string? email = null,
        string? team = null) =>
        User.Create(
            id ?? Guid.NewGuid(),
            userName ?? UserName,
            email ?? Email,
            PasswordHash,
            team ?? Team,
            DateTime.UtcNow);

    /// <summary>A user read model, as a handler returns it.</summary>
    public static UserDto Dto(Guid? id = null) =>
        new(id ?? Guid.NewGuid(), UserName, Email, Team, DateTime.UtcNow, Active: true);

    public static string EmailAlreadyRegistered => $"Email '{Email}' is already registered.";

    public static string UserNameAlreadyTaken => $"User name '{UserName}' is already taken.";
}
