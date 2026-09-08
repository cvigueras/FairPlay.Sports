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
    public static readonly Guid TeamId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public const string PhotoContentType = "image/png";
    public static byte[] PhotoBytes => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static RegisterUserCommand Command() => new(UserName, Email, Password, TeamId);

    public static User DomainUser(
        Guid? id = null,
        string? userName = null,
        string? email = null,
        Guid? teamId = null,
        bool withTeam = true,
        UserRole role = UserRole.Member,
        bool active = true)
    {
        var user = User.Create(
            id ?? Guid.NewGuid(),
            userName ?? UserName,
            email ?? Email,
            PasswordHash,
            withTeam ? teamId ?? TeamId : null,
            DateTime.UtcNow,
            role);

        if (active)
            user.Activate();

        return user;
    }

    public static User DomainUserWithPhoto(Guid? id = null)
    {
        var user = DomainUser(id);
        user.SetPhoto(PhotoBytes, PhotoContentType);
        return user;
    }

    public static UserDto Dto(Guid? id = null, bool hasPhoto = false) =>
        new(id ?? Guid.NewGuid(), UserName, Email, TeamId, UserRole.Member, DateTime.UtcNow, Active: true, HasPhoto: hasPhoto);

    public static string EmailAlreadyRegistered => $"Email '{Email}' is already registered.";

    public static string UserNameAlreadyTaken => $"User name '{UserName}' is already taken.";
}
