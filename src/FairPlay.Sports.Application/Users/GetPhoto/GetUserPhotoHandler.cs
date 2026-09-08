using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.GetPhoto;

public sealed class GetUserPhotoHandler(IUserRepository repository) : IRequestHandler<GetUserPhotoQuery, Result<UserPhoto>>
{
    private readonly IUserRepository _repository = repository;

    public async Task<Result<UserPhoto>> Handle(GetUserPhotoQuery request, CancellationToken cancellationToken)
    {
        var photo = await _repository.GetPhotoAsync(request.UserId, cancellationToken);

        return photo is null
            ? Result<UserPhoto>.NotFound($"User '{request.UserId}' has no photo.")
            : Result<UserPhoto>.Success(photo);
    }
}
