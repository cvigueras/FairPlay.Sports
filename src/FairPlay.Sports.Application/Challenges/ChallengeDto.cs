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
    Guid HomeTeamId,
    Guid AwayTeamId,
    DateTime MatchDate,
    string? VenueName,
    string? VenueAddress,
    PitchSurface? VenueSurface,
    string? VenueMapsUrl,
    ChallengeKitDto HomeKit,
    ChallengeKitDto AwayKit,
    string? Message,
    ChallengeStatus Status,
    DateTime CreatedAt,
    DateTime? RespondedAt)
{
    public static ChallengeDto FromDomain(Challenge challenge, Team challengerTeam, Team challengedTeam)
    {
        var homeTeam = challenge.HomeTeamId == challengerTeam.Id ? challengerTeam : challengedTeam;
        var awayTeam = challenge.HomeTeamId == challengerTeam.Id ? challengedTeam : challengerTeam;
        var awayColors = challenge.AwayKitSlot == TeamKitSlot.First ? awayTeam.Colors! : awayTeam.AlternateColors!;

        return new(
            challenge.Id,
            challenge.ChallengerTeamId,
            challengerTeam.Name,
            challengerTeam.HasCrest,
            challenge.ChallengedTeamId,
            challengedTeam.Name,
            challengedTeam.HasCrest,
            challenge.HomeTeamId,
            challenge.AwayTeamId,
            challenge.MatchDate,
            homeTeam.HomeVenue?.Name,
            homeTeam.HomeVenue?.Address,
            homeTeam.HomeVenue?.Surface,
            homeTeam.HomeVenue?.MapsUrl,
            ChallengeKitDto.FromDomain(homeTeam.Colors!, TeamKitSlot.First),
            ChallengeKitDto.FromDomain(awayColors, challenge.AwayKitSlot),
            challenge.Message,
            challenge.Status,
            challenge.CreatedAt,
            challenge.RespondedAt);
    }
}
