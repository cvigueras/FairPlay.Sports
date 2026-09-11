namespace FairPlay.Sports.Api.Standings;

public sealed record CreateStandingRequest(
    Guid TeamId,
    int Points = default,
    int Played = default,
    int Won = default,
    int Drawn = default,
    int Lost = default,
    int GoalsFor = default,
    int GoalsAgainst = default);
