using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.ChangePassword;

public sealed record ChangeUserPasswordCommand(Guid UserId, string CurrentPassword, string NewPassword)
    : IRequest<Result>;
