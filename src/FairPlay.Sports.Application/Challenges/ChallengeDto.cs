using FairPlay.Sports.Domain.Challenges;
using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Challenges;

public sealed record ChallengeDto(
    Guid Id,
    Guid ChallengerTeamId,
    string ChallengerTeamName,
    bool ChallengerTeamHasCrest,
    Guid ChallengedTeamId,
    string ChallengedTeamName,
    bool ChallengedTeamHasCrest,
    string? Message,
    ChallengeStatus Status,
    DateTime CreatedAt,
    DateTime? RespondedAt)
{
    public static ChallengeDto FromDomain(Challenge challenge, Team challengerTeam, Team challengedTeam) =>
        new(
            challenge.Id,
            challenge.ChallengerTeamId,
            challengerTeam.Name,
            challengerTeam.HasCrest,
            challenge.ChallengedTeamId,
            challengedTeam.Name,
            challengedTeam.HasCrest,
            challenge.Message,
            challenge.Status,
            challenge.CreatedAt,
            challenge.RespondedAt);
}
