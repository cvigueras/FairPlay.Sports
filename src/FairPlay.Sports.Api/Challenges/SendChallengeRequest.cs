using FairPlay.Sports.Domain.Challenges;

namespace FairPlay.Sports.Api.Challenges;

public sealed record SendChallengeRequest(
    Guid ChallengerTeamId,
    Guid ChallengedTeamId,
    Guid VenueTeamId,
    DateTime MatchDate,
    string? Message,
    TeamKitSlot? ChallengerKitPreference = null);
