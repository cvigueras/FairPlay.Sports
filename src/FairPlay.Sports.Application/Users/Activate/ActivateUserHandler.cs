using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Users.Activate;

/// <summary>
/// Turns a user active. Users are created inactive and only become able to sign in
/// once they have been activated. Idempotent: activating an already-active user succeeds.
/// The change is committed by <c>UnitOfWorkBehavior</c> because the command succeeds.
/// </summary>
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
