using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Domain.Teams;
using MediatR;

namespace FairPlay.Sports.Application.Teams.JoinTeam;

public sealed class JoinTeamHandler(
    ITeamRepository teams,
    IUserRepository users,
    ITeamMemberRepository members,
    IClock clock) : IRequestHandler<JoinTeamCommand, Result<TeamMemberDto>>
{
    private readonly ITeamRepository _teams = teams;
    private readonly IUserRepository _users = users;
    private readonly ITeamMemberRepository _members = members;
    private readonly IClock _clock = clock;

    public async Task<Result<TeamMemberDto>> Handle(JoinTeamCommand request, CancellationToken cancellationToken)
    {
        if (!await _teams.ExistsByIdAsync(request.TeamId, cancellationToken))
            return Result<TeamMemberDto>.NotFound($"Team '{request.TeamId}' was not found.");

        var user = await _users.GetByIdForUpdateAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result<TeamMemberDto>.NotFound($"User '{request.UserId}' was not found.");

        if (await _members.ExistsForUserAndTeamAsync(request.TeamId, request.UserId, cancellationToken))
            return Result<TeamMemberDto>.Failure("This user already belongs to this team.");

        if (request.Role != TeamMemberRole.Player &&
            await _members.ExistsWithRoleAsync(request.TeamId, request.Role, cancellationToken))
            return Result<TeamMemberDto>.Failure($"This team already has a {request.Role}.");

        var member = TeamMember.Create(
            Guid.NewGuid(), request.TeamId, request.UserId, request.Role, request.DisplayName, _clock.UtcNow);

        await _members.AddAsync(member, cancellationToken);

        // Belonging to a team is what activates the user.
        user.Activate();

        return Result<TeamMemberDto>.Success(TeamMemberDto.FromDomain(member));
    }
}
