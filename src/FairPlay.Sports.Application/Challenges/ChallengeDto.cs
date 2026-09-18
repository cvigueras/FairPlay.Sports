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
    ChallengeKitDto? HomeKit,
    ChallengeKitDto? AwayKit,
    string? Message,
    ChallengeStatus Status,
    DateTime CreatedAt,
    DateTime? RespondedAt)
{
    /// <summary>
    /// Both kits are known and their primary colour matches - shown as an informational note,
    /// never something that blocks sending or responding to the challenge.
    /// </summary>
    public bool KitsClash =>
        HomeKit is not null && AwayKit is not null &&
        string.Equals(HomeKit.ColorPrimary, AwayKit.ColorPrimary, StringComparison.OrdinalIgnoreCase);

    public static ChallengeDto FromDomain(Challenge challenge, Team challengerTeam, Team challengedTeam)
    {
        var homeTeam = challenge.HomeTeamId == challengerTeam.Id ? challengerTeam : challengedTeam;
        var awayTeam = challenge.HomeTeamId == challengerTeam.Id ? challengedTeam : challengerTeam;

        var homeColors = challenge.HomeKitSlot switch
        {
            TeamKitSlot.First => homeTeam.Colors,
            TeamKitSlot.Second => homeTeam.AlternateColors,
            _ => null,
        };
        var homeKit = homeColors is not null
            ? ChallengeKitDto.FromDomain(homeColors, challenge.HomeKitSlot!.Value)
            : null;
        var awayColors = challenge.AwayKitSlot switch
        {
            TeamKitSlot.First => awayTeam.Colors,
            TeamKitSlot.Second => awayTeam.AlternateColors,
            _ => null,
        };
        var awayKit = awayColors is not null
            ? ChallengeKitDto.FromDomain(awayColors, challenge.AwayKitSlot!.Value)
            : null;

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
            homeKit,
            awayKit,
            challenge.Message,
            challenge.Status,
            challenge.CreatedAt,
            challenge.RespondedAt);
    }
}
