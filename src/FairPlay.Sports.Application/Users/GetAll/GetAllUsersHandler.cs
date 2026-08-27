using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.GetAll;

public sealed class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, Result<IReadOnlyList<UserDto>>>
{
    private readonly IUserRepository _users;

    public GetAllUsersHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Result<IReadOnlyList<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken = default)
    {
        var users = await _users.GetAllAsync(cancellationToken);
        IReadOnlyList<UserDto> dtos = users.Select(UserDto.FromDomain).ToList();

        return Result<IReadOnlyList<UserDto>>.Success(dtos);
    }
}
