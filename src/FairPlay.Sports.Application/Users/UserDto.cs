using FairPlay.Sports.Domain.Users;

namespace FairPlay.Sports.Application.Users;

/// <summary>Read model for a user. Never carries the password hash.</summary>
public sealed record UserDto(
    Guid Id,
    string UserName,
    string Email,
    string Team,
    DateTime CreatedAt,
    bool Active)
{
    public static UserDto FromDomain(User user) =>
        new(user.Id, user.UserName, user.Email, user.Team, user.CreatedAt, user.Active);
}
