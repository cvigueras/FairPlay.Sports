using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.GetById;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>;
