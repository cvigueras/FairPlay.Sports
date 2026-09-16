using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.GetMembers;

public sealed class GetTeamMembersHandler(ITeamRepository teams, ITeamMemberRepository members)
    : IRequestHandler<GetTeamMembersQuery, Result<IReadOnlyList<TeamMemberDto>>>
{
    private readonly ITeamRepository _teams = teams;
    private readonly ITeamMemberRepository _members = members;

    public async Task<Result<IReadOnlyList<TeamMemberDto>>> Handle(
        GetTeamMembersQuery request, CancellationToken cancellationToken)
    {
        if (!await _teams.ExistsByIdAsync(request.TeamId, cancellationToken))
            return Result<IReadOnlyList<TeamMemberDto>>.NotFound($"Team '{request.TeamId}' was not found.");

        var result = (await _members.GetByTeamIdAsync(request.TeamId, cancellationToken))
            .Select(TeamMemberDto.FromDomain)
            .ToList();

        return Result<IReadOnlyList<TeamMemberDto>>.Success(result);
    }
}
