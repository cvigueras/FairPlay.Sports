using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.UploadPhoto;

public sealed record UploadUserPhotoCommand(Guid UserId, byte[] Content, string ContentType)
    : IRequest<Result>;
