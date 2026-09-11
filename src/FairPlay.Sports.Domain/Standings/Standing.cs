namespace FairPlay.Sports.Domain.Standings;

public sealed class Standing
{
    public Guid Id { get; }
    public Guid TeamId { get; }
    public int Points { get; private set; }
    public int Played { get; private set; }
    public int Won { get; private set; }
    public int Drawn { get; private set; }
    public int Lost { get; private set; }
    public int GoalsFor { get; private set; }
    public int GoalsAgainst { get; private set; }

    public DateTime CreatedAt { get; private init; }

    private Standing(Guid id, Guid teamId)
    {
        Id = id;
        TeamId = teamId;
    }

    public static Standing Create(
        Guid id,
        Guid teamId,
        int points,
        int played,
        int won,
        int drawn,
        int lost,
        int goalsFor,
        int goalsAgainst,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Standing id cannot be empty.", nameof(id));

        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));

        var standing = new Standing(id, teamId) { CreatedAt = createdAtUtc };
        standing.ApplyStats(points, played, won, drawn, lost, goalsFor, goalsAgainst);
        return standing;
    }

    public void UpdateStats(int points, int played, int won, int drawn, int lost, int goalsFor, int goalsAgainst) =>
        ApplyStats(points, played, won, drawn, lost, goalsFor, goalsAgainst);

    private void ApplyStats(int points, int played, int won, int drawn, int lost, int goalsFor, int goalsAgainst)
    {
        if (points < 0)
            throw new ArgumentException("Points cannot be negative.", nameof(points));

        if (played < 0)
            throw new ArgumentException("Played cannot be negative.", nameof(played));

        if (won < 0)
            throw new ArgumentException("Won cannot be negative.", nameof(won));

        if (drawn < 0)
            throw new ArgumentException("Drawn cannot be negative.", nameof(drawn));

        if (lost < 0)
            throw new ArgumentException("Lost cannot be negative.", nameof(lost));

        if (goalsFor < 0)
            throw new ArgumentException("Goals for cannot be negative.", nameof(goalsFor));

        if (goalsAgainst < 0)
            throw new ArgumentException("Goals against cannot be negative.", nameof(goalsAgainst));

        if (won + drawn + lost != played)
            throw new ArgumentException("Played must equal won + drawn + lost.", nameof(played));

        Points = points;
        Played = played;
        Won = won;
        Drawn = drawn;
        Lost = lost;
        GoalsFor = goalsFor;
        GoalsAgainst = goalsAgainst;
    }
}
