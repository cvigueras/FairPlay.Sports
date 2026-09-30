using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Api.Users;

public sealed record RegisterUserRequest(
    string UserName,
    string FirstName,
    string LastName,
    string Email,
    string Password,
    TeamMemberRole PrimaryRole,
    bool AcceptedPrivacyPolicy,
    bool ConfirmedMinimumAge);
