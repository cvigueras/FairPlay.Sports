using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.Domain.Users;

namespace FairPlay.Sports.Application.Users;

/// <summary>Read model for a user. Never carries the password hash.</summary>
public sealed record UserDto(
    Guid Id,
    string UserName,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    TeamMemberRole PrimaryRole,
    DateTime CreatedAt,
    bool Active,
    bool HasPhoto)
{
    public static UserDto FromDomain(User user) =>
        new(
            user.Id,
            user.UserName,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role,
            user.PrimaryRole,
            user.CreatedAt,
            user.Active,
            user.HasPhoto);
}
