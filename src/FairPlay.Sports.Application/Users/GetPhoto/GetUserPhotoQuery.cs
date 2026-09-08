using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.GetPhoto;

public sealed record GetUserPhotoQuery(Guid UserId) : IRequest<Result<UserPhoto>>;
