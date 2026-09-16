using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.LeaveTeam;

public sealed class LeaveTeamHandler(ITeamMemberRepository members) : IRequestHandler<LeaveTeamCommand, Result>
{
    private readonly ITeamMemberRepository _members = members;

    public async Task<Result> Handle(LeaveTeamCommand request, CancellationToken cancellationToken)
    {
        var member = await _members.GetByTeamAndUserForUpdateAsync(request.TeamId, request.UserId, cancellationToken);
        if (member is null)
            return Result.NotFound($"User '{request.UserId}' is not a member of team '{request.TeamId}'.");

        _members.Remove(member);

        return Result.Success();
    }
}
