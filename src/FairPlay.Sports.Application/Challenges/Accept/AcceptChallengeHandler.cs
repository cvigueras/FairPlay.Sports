using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Domain.Challenges;
using MediatR;

namespace FairPlay.Sports.Application.Challenges.Accept;

public sealed class AcceptChallengeHandler(
    IChallengeRepository challenges,
    ITeamRepository teams,
    ITeamMemberRepository teamMembers,
    IClock clock) : IRequestHandler<AcceptChallengeCommand, Result<ChallengeDto>>
{
    private readonly IChallengeRepository _challenges = challenges;
    private readonly ITeamRepository _teams = teams;
    private readonly ITeamMemberRepository _teamMembers = teamMembers;
    private readonly IClock _clock = clock;

    public async Task<Result<ChallengeDto>> Handle(AcceptChallengeCommand request, CancellationToken cancellationToken)
    {
        var challenge = await _challenges.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (challenge is null)
            return Result<ChallengeDto>.NotFound($"Challenge '{request.Id}' was not found.");

        var member = await _teamMembers.GetByTeamAndUserAsync(
            challenge.ChallengedTeamId, request.ActingUserId, cancellationToken);
        if (member is null || !ChallengeAuthorization.CanActForTeam(member.Role))
        {
            return Result<ChallengeDto>.Failure(
                "Only the challenged team's delegate, coach, president or technical staff can respond to a challenge.");
        }

        if (challenge.Status != ChallengeStatus.Pending)
            return Result<ChallengeDto>.Failure("This challenge has already been responded to.");

        challenge.Accept(_clock.UtcNow);

        var challengerTeam = await _teams.GetByIdAsync(challenge.ChallengerTeamId, cancellationToken);
        var challengedTeam = await _teams.GetByIdAsync(challenge.ChallengedTeamId, cancellationToken);

        return Result<ChallengeDto>.Success(ChallengeDto.FromDomain(challenge, challengerTeam!, challengedTeam!));
    }
}
