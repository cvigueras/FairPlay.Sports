using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.Register;
using FairPlay.Sports.Domain.Users;

namespace FairPlay.Sports.TestSupport.Users;

public static class UserMother
{
    public const string UserName = "carlos";
    public const string Email = "carlos@example.com";
    public const string Password = "not-a-real-password";
    public const string PasswordHash = "hashed-password";
    public const string Team = "FairPlay FC";

    public static RegisterUserCommand Command() => new(UserName, Email, Password, Team);

    public static User DomainUser(
        Guid? id = null,
        string? userName = null,
        string? email = null,
        string? team = null,
        UserRole role = UserRole.Member,
        bool active = true)
    {
        var user = User.Create(
            id ?? Guid.NewGuid(),
            userName ?? UserName,
            email ?? Email,
            PasswordHash,
            team ?? Team,
            DateTime.UtcNow,
            role);

        if (active)
            user.Activate();

        return user;
    }

    public static UserDto Dto(Guid? id = null) =>
        new(id ?? Guid.NewGuid(), UserName, Email, Team, UserRole.Member, DateTime.UtcNow, Active: true);

    public static string EmailAlreadyRegistered => $"Email '{Email}' is already registered.";

    public static string UserNameAlreadyTaken => $"User name '{UserName}' is already taken.";
}
