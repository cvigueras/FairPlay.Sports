using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Api.Teams;

public sealed record CreateTeamRequest(
    string Name,
    string Coach,
    string City,
    FootballType Type = default,
    Division Division = default,
    AgeCategory Category = default);
