using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Api.Teams;

public sealed record CreateTeamRequest(
    string Name,
    string Coach,
    string City,
    FootballType Type,
    Division Division,
    AgeCategory Category);
