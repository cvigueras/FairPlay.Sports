using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Domain.Challenges;
using MediatR;

namespace FairPlay.Sports.Application.Challenges.Send;

public sealed class SendChallengeHandler(
    IChallengeRepository challenges,
    ITeamRepository teams,
    ITeamMemberRepository teamMembers,
    IClock clock) : IRequestHandler<SendChallengeCommand, Result<ChallengeDto>>
{
    private readonly IChallengeRepository _challenges = challenges;
    private readonly ITeamRepository _teams = teams;
    private readonly ITeamMemberRepository _teamMembers = teamMembers;
    private readonly IClock _clock = clock;

    public async Task<Result<ChallengeDto>> Handle(SendChallengeCommand request, CancellationToken cancellationToken)
    {
        var challengerTeam = await _teams.GetByIdAsync(request.ChallengerTeamId, cancellationToken);
        if (challengerTeam is null)
            return Result<ChallengeDto>.NotFound($"Team '{request.ChallengerTeamId}' was not found.");

        var challengedTeam = await _teams.GetByIdAsync(request.ChallengedTeamId, cancellationToken);
        if (challengedTeam is null)
            return Result<ChallengeDto>.NotFound($"Team '{request.ChallengedTeamId}' was not found.");

        var member = await _teamMembers.GetByTeamAndUserAsync(
            request.ChallengerTeamId, request.ActingUserId, cancellationToken);
        if (member is null || !ChallengeAuthorization.CanActForTeam(member.Role))
        {
            return Result<ChallengeDto>.Failure(
                "Only the challenger team's delegate, coach or president can send a challenge.");
        }

        var challenge = Challenge.Create(
            Guid.NewGuid(), request.ChallengerTeamId, request.ChallengedTeamId, request.Message, _clock.UtcNow);

        await _challenges.AddAsync(challenge, cancellationToken);

        return Result<ChallengeDto>.Success(ChallengeDto.FromDomain(challenge, challengerTeam, challengedTeam));
    }
}
