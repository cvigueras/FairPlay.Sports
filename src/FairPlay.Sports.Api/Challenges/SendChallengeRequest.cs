namespace FairPlay.Sports.Api.Challenges;

public sealed record SendChallengeRequest(Guid ChallengerTeamId, Guid ChallengedTeamId, string? Message);
