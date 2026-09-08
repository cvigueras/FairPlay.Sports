using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.UploadPhoto;

public sealed class UploadUserPhotoHandler(IUserRepository repository) : IRequestHandler<UploadUserPhotoCommand, Result>
{
    private readonly IUserRepository _repository = repository;

    public async Task<Result> Handle(UploadUserPhotoCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdForUpdateAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.NotFound($"User '{request.UserId}' was not found.");

        user.SetPhoto(request.Content, request.ContentType);

        return Result.Success();
    }
}
