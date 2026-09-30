using FairPlay.Sports.Application.Challenges;
using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.SetVenueAvailable;

public sealed class SetVenueAvailableHandler(
    ITeamRepository teams,
    ITeamMemberRepository teamMembers) : IRequestHandler<SetVenueAvailableCommand, Result<TeamDto>>
{
    private readonly ITeamRepository _teams = teams;
    private readonly ITeamMemberRepository _teamMembers = teamMembers;

    public async Task<Result<TeamDto>> Handle(SetVenueAvailableCommand request, CancellationToken cancellationToken)
    {
        var team = await _teams.GetByIdForUpdateAsync(request.TeamId, cancellationToken);
        if (team is null)
            return Result<TeamDto>.NotFound($"Team '{request.TeamId}' was not found.");

        var member = await _teamMembers.GetByTeamAndUserAsync(request.TeamId, request.ActingUserId, cancellationToken);
        if (member is null || !ChallengeAuthorization.CanActForTeam(member.Role))
        {
            return Result<TeamDto>.Failure(
                "Only the team's delegate, coach, president or technical staff can change whether its venue is available.");
        }

        team.SetVenueAvailable(request.Available);

        return Result<TeamDto>.Success(TeamDto.FromDomain(team));
    }
}
