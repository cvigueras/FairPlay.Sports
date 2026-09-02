namespace FairPlay.Sports.Domain.Teams;

/// <summary>Competitive tier a team plays in, from the top down.</summary>
public enum Division
{
    Default,
    /// <summary>División de Honor.</summary>
    HonorDivision,

    /// <summary>Liga Autonómica / Regional.</summary>
    RegionalLeague,

    /// <summary>Primera (Regional).</summary>
    First,

    /// <summary>Segunda (Regional).</summary>
    Second
}
