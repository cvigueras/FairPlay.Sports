namespace FairPlay.Sports.Domain.Teams;

/// <summary>
/// Grassroots age bracket. Named with the international U-notation so it reads
/// the same across modalities; the Spanish label each one maps to is noted.
/// </summary>
public enum AgeCategory
{
    Default,
    /// <summary>Juvenil (16-18).</summary>
    Under19,

    /// <summary>Cadete (14-15).</summary>
    Under16,

    /// <summary>Infantil (12-13).</summary>
    Under14,

    /// <summary>Alevín (10-11).</summary>
    Under12,

    /// <summary>Benjamín (8-9).</summary>
    Under10,

    /// <summary>Prebenjamín (6-7).</summary>
    Under8,

    /// <summary>Debutante (4-5).</summary>
    Under6
}
