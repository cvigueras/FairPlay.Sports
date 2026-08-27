using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.GetById;

public sealed class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _users;

    public GetUserByIdHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(request.Id, cancellationToken);

        return user is null
            ? Result<UserDto>.NotFound($"User '{request.Id}' was not found.")
            : Result<UserDto>.Success(UserDto.FromDomain(user));
    }
}
