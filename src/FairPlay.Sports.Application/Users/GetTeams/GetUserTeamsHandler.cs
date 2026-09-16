using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using MediatR;

namespace FairPlay.Sports.Application.Users.GetTeams;

public sealed class GetUserTeamsHandler(IUserRepository users, ITeamMemberRepository members)
    : IRequestHandler<GetUserTeamsQuery, Result<IReadOnlyList<TeamMemberDto>>>
{
    private readonly IUserRepository _users = users;
    private readonly ITeamMemberRepository _members = members;

    public async Task<Result<IReadOnlyList<TeamMemberDto>>> Handle(
        GetUserTeamsQuery request, CancellationToken cancellationToken)
    {
        if (await _users.GetByIdAsync(request.UserId, cancellationToken) is null)
            return Result<IReadOnlyList<TeamMemberDto>>.NotFound($"User '{request.UserId}' was not found.");

        var result = (await _members.GetByUserIdAsync(request.UserId, cancellationToken))
            .Select(TeamMemberDto.FromDomain)
            .ToList();

        return Result<IReadOnlyList<TeamMemberDto>>.Success(result);
    }
}
