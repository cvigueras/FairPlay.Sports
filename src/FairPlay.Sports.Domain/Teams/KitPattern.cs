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

    /// <summary>Horizontal stripes ("hoops") confined to the torso.</summary>
    Hoops,

    /// <summary>Horizontal stripes covering the whole shirt, collar to hem.</summary>
    FullHoops,

    /// <summary>Left/right halves in each colour.</summary>
    Halves,

    /// <summary>Diagonal band ("banda") across the primary colour.</summary>
    Sash,

    /// <summary>Checkered pattern.</summary>
    Checkered,

    /// <summary>Sleeves in the secondary colour, body in the primary.</summary>
    Sleeves,

    /// <summary>Gradient fade from the primary colour into the secondary.</summary>
    Fade
}
