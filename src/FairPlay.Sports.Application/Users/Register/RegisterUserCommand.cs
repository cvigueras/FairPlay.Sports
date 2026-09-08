using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.Register;

public sealed record RegisterUserCommand(string UserName, string Email, string Password, Guid? TeamId = null)
    : IRequest<Result<UserDto>>;
