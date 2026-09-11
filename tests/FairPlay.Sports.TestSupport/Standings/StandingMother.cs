using FairPlay.Sports.Application.Standings;
using FairPlay.Sports.Application.Standings.Create;
using FairPlay.Sports.Application.Standings.Update;
using FairPlay.Sports.Domain.Standings;

namespace FairPlay.Sports.TestSupport.Standings;

public static class StandingMother
{
    public const int Points = 10;
    public const int Played = 5;
    public const int Won = 3;
    public const int Drawn = 1;
    public const int Lost = 1;
    public const int GoalsFor = 12;
    public const int GoalsAgainst = 7;

    public static CreateStandingCommand Command(Guid? teamId = null) =>
        new(teamId ?? Guid.NewGuid(), Points, Played, Won, Drawn, Lost, GoalsFor, GoalsAgainst);

    public static UpdateStandingCommand UpdateCommand(Guid id) =>
        new(id, Points, Played, Won, Drawn, Lost, GoalsFor, GoalsAgainst);

    public static Standing DomainStanding(
        Guid? id = null,
        Guid? teamId = null,
        int points = Points,
        int played = Played,
        int won = Won,
        int drawn = Drawn,
        int lost = Lost,
        int goalsFor = GoalsFor,
        int goalsAgainst = GoalsAgainst,
        DateTime? createdAtUtc = null) =>
        Standing.Create(
            id ?? Guid.NewGuid(),
            teamId ?? Guid.NewGuid(),
            points, played, won, drawn, lost, goalsFor, goalsAgainst,
            createdAtUtc ?? DateTime.UtcNow);

    public static StandingDto Dto(Guid? id = null, Guid? teamId = null, string teamName = "FairPlay FC") =>
        new(
            id ?? Guid.NewGuid(),
            teamId ?? Guid.NewGuid(),
            teamName,
            TeamHasCrest: false,
            Points, Played, Won, Drawn, Lost, GoalsFor, GoalsAgainst,
            DateTime.UtcNow);

    public static string TeamAlreadyHasStanding(Guid teamId) => $"Team '{teamId}' already has a standing.";
}
