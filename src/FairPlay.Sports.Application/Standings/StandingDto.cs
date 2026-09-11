using FairPlay.Sports.Domain.Standings;
using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Standings;

public sealed record StandingDto(
    Guid Id,
    Guid TeamId,
    string TeamName,
    bool TeamHasCrest,
    int Points,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int GoalsFor,
    int GoalsAgainst,
    DateTime CreatedAt)
{
    public int GoalDifference => GoalsFor - GoalsAgainst;

    public static StandingDto FromDomain(Standing standing, Team team) =>
        new(
            standing.Id,
            standing.TeamId,
            team.Name,
            team.HasCrest,
            standing.Points,
            standing.Played,
            standing.Won,
            standing.Drawn,
            standing.Lost,
            standing.GoalsFor,
            standing.GoalsAgainst,
            standing.CreatedAt);
}
