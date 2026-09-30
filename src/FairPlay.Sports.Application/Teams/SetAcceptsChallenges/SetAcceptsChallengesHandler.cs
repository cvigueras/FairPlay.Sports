using FairPlay.Sports.Application.Challenges;
using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.SetAcceptsChallenges;

public sealed class SetAcceptsChallengesHandler(
    ITeamRepository teams,
    ITeamMemberRepository teamMembers) : IRequestHandler<SetAcceptsChallengesCommand, Result<TeamDto>>
{
    private readonly ITeamRepository _teams = teams;
    private readonly ITeamMemberRepository _teamMembers = teamMembers;

    public async Task<Result<TeamDto>> Handle(SetAcceptsChallengesCommand request, CancellationToken cancellationToken)
    {
        var team = await _teams.GetByIdForUpdateAsync(request.TeamId, cancellationToken);
        if (team is null)
            return Result<TeamDto>.NotFound($"Team '{request.TeamId}' was not found.");

        var member = await _teamMembers.GetByTeamAndUserAsync(request.TeamId, request.ActingUserId, cancellationToken);
        if (member is null || !ChallengeAuthorization.CanActForTeam(member.Role))
        {
            return Result<TeamDto>.Failure(
                "Only the team's delegate, coach, president or technical staff can change whether it accepts challenges.");
        }

        team.SetAcceptsChallenges(request.Accepts);

        return Result<TeamDto>.Success(TeamDto.FromDomain(team));
    }
}
