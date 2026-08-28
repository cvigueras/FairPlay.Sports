using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.GetById;

public sealed class GetUserByIdHandler(IUserRepository repository) : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _repository = repository;

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.Id, cancellationToken);

        return user is null
            ? Result<UserDto>.NotFound($"User '{request.Id}' was not found.")
            : Result<UserDto>.Success(UserDto.FromDomain(user));
    }
}
