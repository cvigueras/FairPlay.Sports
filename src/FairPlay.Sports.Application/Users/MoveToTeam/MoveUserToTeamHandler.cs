using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using MediatR;

namespace FairPlay.Sports.Application.Users.MoveToTeam;

public sealed class MoveUserToTeamHandler(IUserRepository users, ITeamRepository teams)
    : IRequestHandler<MoveUserToTeamCommand, Result<UserDto>>
{
    private readonly IUserRepository _users = users;
    private readonly ITeamRepository _teams = teams;

    public async Task<Result<UserDto>> Handle(MoveUserToTeamCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdForUpdateAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result<UserDto>.NotFound($"User '{request.UserId}' was not found.");

        if (!await _teams.ExistsByIdAsync(request.TeamId, cancellationToken))
            return Result<UserDto>.NotFound($"Team '{request.TeamId}' was not found.");

        user.MoveToTeam(request.TeamId);

        // Belonging to a team is what activates the user.
        user.Activate();

        return Result<UserDto>.Success(UserDto.FromDomain(user));
    }
}
