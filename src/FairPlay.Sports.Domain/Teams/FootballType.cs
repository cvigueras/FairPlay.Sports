namespace FairPlay.Sports.Domain.Teams;

/// <summary>
/// Playing modality of a team. Kept as a closed set so future team-vs-team
/// challenges can only pair teams of the same modality.
/// </summary>
public enum FootballType
{
    Default,
    /// <summary>Fútbol 11 (eleven-a-side).</summary>
    Football11,

    /// <summary>Fútbol 8 (eight-a-side).</summary>
    Football8,

    /// <summary>Fútbol sala (futsal).</summary>
    Futsal,

    /// <summary>Fútbol playa (beach soccer).</summary>
    BeachSoccer
}
