namespace FairPlay.Sports.Domain.Teams;

/// <summary>
/// Visual layout of a team's kit colours - which parts of the shirt the
/// primary and secondary colours cover. Closed set so the frontend can offer
/// a fixed picker instead of free text.
/// </summary>
public enum KitPattern
{
    Default,

    /// <summary>Solid primary colour, no secondary.</summary>
    Plain,

    /// <summary>Vertical stripes.</summary>
    Stripes,

    /// <summary>Horizontal stripes ("hoops").</summary>
    Hoops,

    /// <summary>Left/right halves in each colour.</summary>
    Halves,

    /// <summary>Diagonal band ("banda") across the primary colour.</summary>
    Sash,

    /// <summary>Checkered pattern.</summary>
    Checkered
}
