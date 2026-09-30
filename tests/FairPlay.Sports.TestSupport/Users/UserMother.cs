using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Application.Users.Register;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.Domain.Users;

namespace FairPlay.Sports.TestSupport.Users;

public static class UserMother
{
    public const string UserName = "carlos";
    public const string FirstName = "Carlos";
    public const string LastName = "Vigueras";
    public const string Email = "carlos@example.com";
    public const TeamMemberRole PrimaryRole = TeamMemberRole.Coach;
    public const string PrivacyVersion = "2026-09-30";
    public const string Password = "not-a-real-password";
    public const string PasswordHash = "hashed-password";

    public const string PhotoContentType = "image/png";
    public static byte[] PhotoBytes => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static RegisterUserCommand Command(bool acceptedPrivacyPolicy = true, bool confirmedMinimumAge = true) =>
        new(UserName, FirstName, LastName, Email, Password, PrimaryRole, acceptedPrivacyPolicy, confirmedMinimumAge);

    public static User DomainUser(
        Guid? id = null,
        string? userName = null,
        string? email = null,
        UserRole role = UserRole.Member,
        bool active = true)
    {
        var user = User.Create(
            id ?? Guid.NewGuid(),
            userName ?? UserName,
            email ?? Email,
            PasswordHash,
            DateTime.UtcNow,
            FirstName,
            LastName,
            PrimaryRole,
            PrivacyVersion,
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
        new(
            id ?? Guid.NewGuid(),
            UserName,
            FirstName,
            LastName,
            Email,
            UserRole.Member,
            PrimaryRole,
            DateTime.UtcNow,
            Active: true,
            HasPhoto: hasPhoto);

    public static string EmailAlreadyRegistered => $"Email '{Email}' is already registered.";

    public static string UserNameAlreadyTaken => $"User name '{UserName}' is already taken.";
}
