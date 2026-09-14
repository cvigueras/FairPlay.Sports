using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.UpdateProfile;

public sealed record UpdateUserProfileCommand(Guid UserId, string UserName, string Email)
    : IRequest<Result<UserDto>>;
