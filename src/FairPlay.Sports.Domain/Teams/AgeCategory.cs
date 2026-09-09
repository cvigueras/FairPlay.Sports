namespace FairPlay.Sports.Domain.Teams;

/// <summary>
/// Grassroots age bracket, named with the Spanish federation categories. The
/// age range / birth years in each note are for reference only.
/// </summary>
public enum AgeCategory
{
    Default,

    /// <summary>4-5 años.</summary>
    Chupetes,

    /// <summary>6-7 años (nacidos en 2019-2020).</summary>
    Prebenjamines,

    /// <summary>8-9 años (nacidos en 2017-2018).</summary>
    Benjamines,

    /// <summary>10-11 años (nacidos en 2015-2016).</summary>
    Alevines,

    /// <summary>12-13 años (nacidos en 2013-2014).</summary>
    Infantiles,

    /// <summary>14-15 años (nacidos en 2011-2012).</summary>
    Cadetes,

    /// <summary>16-18 años (nacidos en 2008-2010).</summary>
    Juveniles,

    /// <summary>A partir de 19 años (nacidos en 2007 y anteriores).</summary>
    Aficionados,

    /// <summary>Generalmente a partir de los 30-35 años.</summary>
    Veteranos
}
