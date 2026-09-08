using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.MoveToTeam;

public sealed record MoveUserToTeamCommand(Guid UserId, Guid TeamId) : IRequest<Result<UserDto>>;
