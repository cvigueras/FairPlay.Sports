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

        var challengerIsHome = request.VenueTeamId == challengerTeam.Id;

        // The challenger wears its first kit by default; it may pick its second instead,
        // whether it's playing at home or away, as long as it actually has that kit
        // configured - otherwise the default applies as usual. The challenged team never
        // picks; its kit is always resolved automatically to avoid clashing with whatever
        // the challenger ends up wearing.
        var challengerKitSlot = request.ChallengerKitPreference is not null &&
            HasKit(challengerTeam, request.ChallengerKitPreference.Value)
                ? request.ChallengerKitPreference.Value
                : ResolveDefaultKitSlot(challengerTeam);
        var challengerColors = challengerKitSlot == TeamKitSlot.First ? challengerTeam.Colors
            : challengerKitSlot == TeamKitSlot.Second ? challengerTeam.AlternateColors
            : null;

        // Best-effort: missing kits or an unavoidable colour clash never block sending, they
        // just leave the challenged team's slot unresolved (or picked despite the clash) for
        // the DTO to flag.
        var challengedKitSlot = ResolveOpponentKitSlot(challengerColors, challengedTeam);

        var homeKitSlot = challengerIsHome ? challengerKitSlot : challengedKitSlot;
        var awayKitSlot = challengerIsHome ? challengedKitSlot : challengerKitSlot;

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

    /// <summary>A team's first kit, or its second if that's all it has configured.</summary>
    private static TeamKitSlot? ResolveDefaultKitSlot(Team team)
    {
        if (team.Colors is not null) return TeamKitSlot.First;
        if (team.AlternateColors is not null) return TeamKitSlot.Second;
        return null;
    }

    /// <summary>
    /// The opponent's first kit unless its primary colour matches the challenger's; then its
    /// second kit if that avoids the clash; otherwise falls back to its first kit (accepting the
    /// clash) if it has one, else null (no kit configured at all).
    /// </summary>
    private static TeamKitSlot? ResolveOpponentKitSlot(KitColors? challengerColors, Team opponent)
    {
        var first = opponent.Colors;
        var second = opponent.AlternateColors;

        if (challengerColors is null)
            return first is not null ? TeamKitSlot.First : null;

        if (first is not null && !ClashesWith(challengerColors, first))
            return TeamKitSlot.First;

        if (second is not null && !ClashesWith(challengerColors, second))
            return TeamKitSlot.Second;

        return first is not null ? TeamKitSlot.First : null;
    }

    private static bool ClashesWith(KitColors a, KitColors b) =>
        string.Equals(a.Primary, b.Primary, StringComparison.OrdinalIgnoreCase);

    private static bool HasKit(Team team, TeamKitSlot slot) =>
        slot == TeamKitSlot.First ? team.Colors is not null : team.AlternateColors is not null;
}
