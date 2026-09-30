using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Domain.Teams;
using MediatR;

namespace FairPlay.Sports.Application.Users.Register;

public sealed record RegisterUserCommand(
    string UserName,
    string FirstName,
    string LastName,
    string Email,
    string Password,
    TeamMemberRole PrimaryRole,
    bool AcceptedPrivacyPolicy,
    bool ConfirmedMinimumAge)
    : IRequest<Result<UserDto>>;
