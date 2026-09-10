namespace FairPlay.Sports.Domain.Teams;

/// <summary>
/// Playing surface of a team's home venue, as listed on a federation club
/// sheet ("terreno de juego"). Closed set so the frontend can offer a fixed
/// dropdown and pair it with a modality.
/// </summary>
public enum PitchSurface
{
    Default,

    /// <summary>Césped natural.</summary>
    NaturalGrass,

    /// <summary>Césped artificial.</summary>
    ArtificialTurf,

    /// <summary>Césped híbrido.</summary>
    Hybrid,

    /// <summary>Tierra / albero.</summary>
    Earth,

    /// <summary>Pista cubierta (pabellón), típica de fútbol sala.</summary>
    Indoor
}
