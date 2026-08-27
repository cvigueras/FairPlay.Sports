using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.GetAll;

public sealed record GetAllUsersQuery() : IRequest<Result<IReadOnlyList<UserDto>>>;
