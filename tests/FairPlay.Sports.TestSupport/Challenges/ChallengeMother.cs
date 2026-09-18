using FairPlay.Sports.Application.Challenges;
using FairPlay.Sports.Application.Challenges.Accept;
using FairPlay.Sports.Application.Challenges.Reject;
using FairPlay.Sports.Application.Challenges.Send;
using FairPlay.Sports.Domain.Challenges;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.TestSupport.Teams;

namespace FairPlay.Sports.TestSupport.Challenges;

public static class ChallengeMother
{
    public static readonly Guid ChallengerTeamId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid ChallengedTeamId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid ActingUserId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly DateTime MatchDate = new(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc);
    public const string Message = "¿Jugamos el sábado?";

    public static SendChallengeCommand SendCommand(
        Guid? challengerTeamId = null,
        Guid? challengedTeamId = null,
        Guid? venueTeamId = null,
        DateTime? matchDate = null,
        string? message = Message,
        Guid? actingUserId = null,
        TeamKitSlot? challengerKitPreference = null) =>
        new(
            challengerTeamId ?? ChallengerTeamId,
            challengedTeamId ?? ChallengedTeamId,
            venueTeamId ?? challengerTeamId ?? ChallengerTeamId,
            matchDate ?? MatchDate,
            message,
            actingUserId ?? ActingUserId,
            challengerKitPreference);

    public static AcceptChallengeCommand AcceptCommand(Guid? id = null, Guid? actingUserId = null) =>
        new(id ?? Guid.NewGuid(), actingUserId ?? ActingUserId);

    public static RejectChallengeCommand RejectCommand(Guid? id = null, Guid? actingUserId = null) =>
        new(id ?? Guid.NewGuid(), actingUserId ?? ActingUserId);

    public static Challenge DomainChallenge(
        Guid? id = null,
        Guid? challengerTeamId = null,
        Guid? challengedTeamId = null,
        Guid? venueTeamId = null,
        DateTime? matchDate = null,
        TeamKitSlot awayKitSlot = TeamKitSlot.First,
        string? message = Message,
        DateTime? createdAtUtc = null) =>
        Challenge.Create(
            id ?? Guid.NewGuid(),
            challengerTeamId ?? ChallengerTeamId,
            challengedTeamId ?? ChallengedTeamId,
            venueTeamId ?? challengerTeamId ?? ChallengerTeamId,
            matchDate ?? MatchDate,
            awayKitSlot,
            message,
            createdAtUtc ?? DateTime.UtcNow);

    public static TeamMember Member(Guid teamId, Guid userId, TeamMemberRole role) =>
        TeamMember.Create(Guid.NewGuid(), teamId, userId, role, "Carlos V.", DateTime.UtcNow);

    public static ChallengeDto Dto(Guid? id = null) =>
        new(
            id ?? Guid.NewGuid(),
            ChallengerTeamId,
            TeamMother.Name,
            false,
            ChallengedTeamId,
            TeamMother.Name,
            false,
            ChallengerTeamId,
            ChallengedTeamId,
            MatchDate,
            TeamMother.VenueName,
            TeamMother.VenueAddress,
            TeamMother.VenueSurface,
            TeamMother.VenueMapsUrl,
            new ChallengeKitDto(
                TeamMother.ColorPrimary, TeamMother.ColorSecondary, TeamMother.ShortsColor, TeamMother.KitPattern, TeamKitSlot.First),
            new ChallengeKitDto(
                TeamMother.AlternateColorPrimary,
                TeamMother.AlternateColorSecondary,
                TeamMother.AlternateShortsColor,
                TeamMother.AlternateKitPattern,
                TeamKitSlot.First),
            Message,
            ChallengeStatus.Pending,
            DateTime.UtcNow,
            null);

    public static string NotAllowedToSend =>
        "Only the challenger team's delegate, coach, president or technical staff can send a challenge.";

    public static string NotAllowedToRespond =>
        "Only the challenged team's delegate, coach, president or technical staff can respond to a challenge.";

    public static string AlreadyResponded => "This challenge has already been responded to.";

    public static string MatchDateNotInFuture => "The match date must be in the future.";
}
