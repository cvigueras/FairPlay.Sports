using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.Activate;

public sealed record ActivateUserCommand(Guid Id) : IRequest<Result>;
