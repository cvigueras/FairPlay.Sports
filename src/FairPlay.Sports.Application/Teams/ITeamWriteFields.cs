using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Teams;

/// <summary>
/// The full set of fields a create or update command carries for a team,
/// flattened the same way <see cref="TeamDto"/> is. Shared so the validation
/// rules and the domain mapping live in one place for both use cases.
/// </summary>
public interface ITeamWriteFields
{
    string Name { get; }
    string Coach { get; }
    string City { get; }

    FootballType Type { get; }
    Division Division { get; }
    AgeCategory Category { get; }

    string? ShortName { get; }
    int? FoundedYear { get; }

    string? VenueName { get; }
    string? VenueAddress { get; }
    PitchSurface? VenueSurface { get; }
    string? VenueMapsUrl { get; }

    string? ColorPrimary { get; }
    string? ColorSecondary { get; }

    string? ContactEmail { get; }
    string? ContactPhone { get; }
    string? Website { get; }
}
