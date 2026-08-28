using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.GetAll;

public sealed class GetAllUsersHandler(IUserRepository repository) : IRequestHandler<GetAllUsersQuery, Result<IReadOnlyList<UserDto>>>
{
    private readonly IUserRepository _repository = repository;

    public async Task<Result<IReadOnlyList<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var result = (await _repository.GetAllAsync(cancellationToken)).Select(UserDto.FromDomain).ToList();
        return Result<IReadOnlyList<UserDto>>.Success(result);
    }
}
