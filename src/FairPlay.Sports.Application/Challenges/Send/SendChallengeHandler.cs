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

        // The home team wears its first kit by default; the challenger may pick its second
        // instead, but only when it's the one playing at home (the challenged team's kit is
        // never chosen this way) and it actually has that kit configured - otherwise the
        // default applies as usual.
        var homeKitSlot = homeTeam.Id == challengerTeam.Id &&
            request.ChallengerKitPreference is not null &&
            HasKit(homeTeam, request.ChallengerKitPreference.Value)
                ? request.ChallengerKitPreference.Value
                : ResolveHomeKitSlot(homeTeam);
        var homeColors = homeKitSlot == TeamKitSlot.First ? homeTeam.Colors
            : homeKitSlot == TeamKitSlot.Second ? homeTeam.AlternateColors
            : null;

        // Best-effort: missing kits or an unavoidable colour clash never block sending, they
        // just leave AwayKitSlot unresolved (or picked despite the clash) for the DTO to flag.
        // The away team never picks its own kit - it's always resolved automatically against
        // whichever kit the home team ends up wearing.
        var awayKitSlot = ResolveAwayKitSlot(homeColors, awayTeam);

        var challenge = Challenge.Create(
            Guid.NewGuid(),
            request.ChallengerTeamId,
            request.ChallengedTeamId,
            request.VenueTeamId,
            request.MatchDate,
            homeKitSlot,
            awayKitSlot,
            request.Message,
            _clock.UtcNow);

        await _challenges.AddAsync(challenge, cancellationToken);

        return Result<ChallengeDto>.Success(ChallengeDto.FromDomain(challenge, challengerTeam, challengedTeam));
    }

    /// <summary>The home team's first kit, or its second if that's all it has configured.</summary>
    private static TeamKitSlot? ResolveHomeKitSlot(Team homeTeam)
    {
        if (homeTeam.Colors is not null) return TeamKitSlot.First;
        if (homeTeam.AlternateColors is not null) return TeamKitSlot.Second;
        return null;
    }

    /// <summary>
    /// The away team's first kit unless its primary colour matches the home team's; then its
    /// second kit if that avoids the clash; otherwise falls back to its first kit (accepting the
    /// clash) if it has one, else null (no kit configured at all).
    /// </summary>
    private static TeamKitSlot? ResolveAwayKitSlot(KitColors? homeColors, Team awayTeam)
    {
        var first = awayTeam.Colors;
        var second = awayTeam.AlternateColors;

        if (homeColors is null)
            return first is not null ? TeamKitSlot.First : null;

        if (first is not null && !ClashesWith(homeColors, first))
            return TeamKitSlot.First;

        if (second is not null && !ClashesWith(homeColors, second))
            return TeamKitSlot.Second;

        return first is not null ? TeamKitSlot.First : null;
    }

    private static bool ClashesWith(KitColors home, KitColors away) =>
        string.Equals(home.Primary, away.Primary, StringComparison.OrdinalIgnoreCase);

    private static bool HasKit(Team team, TeamKitSlot slot) =>
        slot == TeamKitSlot.First ? team.Colors is not null : team.AlternateColors is not null;
}
