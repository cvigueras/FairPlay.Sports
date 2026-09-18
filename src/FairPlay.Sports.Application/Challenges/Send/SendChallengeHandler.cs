using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Domain.Challenges;
using FairPlay.Sports.Domain.Teams;
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
                "Only the challenger team's delegate, coach, president or technical staff can send a challenge.");
        }

        if (request.MatchDate <= _clock.UtcNow)
            return Result<ChallengeDto>.Failure("The match date must be in the future.");

        var homeTeam = request.VenueTeamId == challengerTeam.Id ? challengerTeam : challengedTeam;
        var awayTeam = request.VenueTeamId == challengerTeam.Id ? challengedTeam : challengerTeam;

        if (homeTeam.Colors is null)
            return Result<ChallengeDto>.Failure("The home team has no kit configured.");

        var awayKitSlot = ResolveAwayKitSlot(homeTeam.Colors, awayTeam);
        if (awayKitSlot is null)
        {
            return Result<ChallengeDto>.Failure(
                "The away team has no kit that avoids clashing with the home team's kit.");
        }

        var challenge = Challenge.Create(
            Guid.NewGuid(),
            request.ChallengerTeamId,
            request.ChallengedTeamId,
            request.VenueTeamId,
            request.MatchDate,
            awayKitSlot.Value,
            request.Message,
            _clock.UtcNow);

        await _challenges.AddAsync(challenge, cancellationToken);

        return Result<ChallengeDto>.Success(ChallengeDto.FromDomain(challenge, challengerTeam, challengedTeam));
    }

    /// <summary>
    /// The away team's first kit unless its primary colour matches the home team's; then its
    /// second kit if that avoids the clash; null if neither does (or there is no second kit).
    /// </summary>
    private static TeamKitSlot? ResolveAwayKitSlot(KitColors homeColors, Team awayTeam)
    {
        if (awayTeam.Colors is not null && !ClashesWith(homeColors, awayTeam.Colors))
            return TeamKitSlot.First;

        if (awayTeam.AlternateColors is not null && !ClashesWith(homeColors, awayTeam.AlternateColors))
            return TeamKitSlot.Second;

        return null;
    }

    private static bool ClashesWith(KitColors home, KitColors away) =>
        string.Equals(home.Primary, away.Primary, StringComparison.OrdinalIgnoreCase);
}
