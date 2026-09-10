namespace FairPlay.Sports.Domain.Teams;

/// <summary>
/// The optional "ficha" details a team may carry beyond its core identity and
/// classification: short name, founding year, home venue, kit colours and
/// contact channels. Not an aggregate - just the group of nullable fields that
/// <see cref="Team.Create"/> and <see cref="Team.Update"/> share. The nested
/// value objects validate themselves; <see cref="Team"/> validates the loose
/// fields when it applies the profile.
/// </summary>
public sealed record TeamProfile(
    string? ShortName = null,
    int? FoundedYear = null,
    Venue? HomeVenue = null,
    KitColors? Colors = null,
    string? ContactEmail = null,
    string? ContactPhone = null,
    string? Website = null)
{
    public static readonly TeamProfile Empty = new();
}
