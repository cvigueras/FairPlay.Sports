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
    public const string Message = "¿Jugamos el sábado?";

    public static SendChallengeCommand SendCommand(
        Guid? challengerTeamId = null, Guid? challengedTeamId = null, string? message = Message, Guid? actingUserId = null) =>
        new(challengerTeamId ?? ChallengerTeamId, challengedTeamId ?? ChallengedTeamId, message, actingUserId ?? ActingUserId);

    public static AcceptChallengeCommand AcceptCommand(Guid? id = null, Guid? actingUserId = null) =>
        new(id ?? Guid.NewGuid(), actingUserId ?? ActingUserId);

    public static RejectChallengeCommand RejectCommand(Guid? id = null, Guid? actingUserId = null) =>
        new(id ?? Guid.NewGuid(), actingUserId ?? ActingUserId);

    public static Challenge DomainChallenge(
        Guid? id = null,
        Guid? challengerTeamId = null,
        Guid? challengedTeamId = null,
        string? message = Message,
        DateTime? createdAtUtc = null) =>
        Challenge.Create(
            id ?? Guid.NewGuid(),
            challengerTeamId ?? ChallengerTeamId,
            challengedTeamId ?? ChallengedTeamId,
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
            Message,
            ChallengeStatus.Pending,
            DateTime.UtcNow,
            null);

    public static string NotAllowedToSend => "Only the challenger team's delegate, coach or president can send a challenge.";

    public static string NotAllowedToRespond =>
        "Only the challenged team's delegate, coach or president can respond to a challenge.";

    public static string AlreadyResponded => "This challenge has already been responded to.";
}
