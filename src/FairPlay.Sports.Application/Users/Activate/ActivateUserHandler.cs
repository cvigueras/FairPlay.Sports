using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using MediatR;

namespace FairPlay.Sports.Application.Users.Activate;

public sealed class ActivateUserHandler(IUserRepository repository, ITeamMemberRepository members)
    : IRequestHandler<ActivateUserCommand, Result>
{
    private readonly IUserRepository _repository = repository;
    private readonly ITeamMemberRepository _members = members;

    public async Task<Result> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (user is null)
            return Result.NotFound($"User '{request.Id}' was not found.");

        if (!await _members.ExistsForUserAsync(request.Id, cancellationToken))
            return Result.Failure("A user cannot be activated until they belong to a team.");

        user.Activate();

        return Result.Success();
    }
}
