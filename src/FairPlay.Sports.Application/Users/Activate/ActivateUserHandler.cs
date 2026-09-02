using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.Activate;

public sealed class ActivateUserHandler(IUserRepository repository) : IRequestHandler<ActivateUserCommand, Result>
{
    private readonly IUserRepository _repository = repository;

    public async Task<Result> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (user is null)
            return Result.NotFound($"User '{request.Id}' was not found.");

        user.Activate();

        return Result.Success();
    }
}
