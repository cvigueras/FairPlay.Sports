using FairPlay.Sports.Domain.Users;

namespace FairPlay.Sports.Application.Users;

/// <summary>Read model for a user. Never carries the password hash.</summary>
public sealed record UserDto(
    Guid Id,
    string UserName,
    string Email,
    Guid? TeamId,
    UserRole Role,
    DateTime CreatedAt,
    bool Active,
    bool HasPhoto)
{
    public static UserDto FromDomain(User user) =>
        new(user.Id, user.UserName, user.Email, user.TeamId, user.Role, user.CreatedAt, user.Active, user.HasPhoto);
}
