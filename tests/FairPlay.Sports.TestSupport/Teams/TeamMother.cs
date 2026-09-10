using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.Create;
using FairPlay.Sports.Application.Teams.Update;
using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.TestSupport.Teams;

public static class TeamMother
{
    public const string Name = "FairPlay FC";
    public const string Coach = "Marta Rios";
    public const string City = "Sevilla";
    public const FootballType DefaultType = FootballType.Futsal;
    public const Division DefaultDivision = Division.First;
    public const AgeCategory DefaultCategory = AgeCategory.Juveniles;

    public const string ShortName = "FPF";
    public const int FoundedYear = 1998;
    public const string VenueName = "Pabellón Municipal";
    public const string VenueAddress = "Calle del Deporte 1, Sevilla";
    public const PitchSurface VenueSurface = PitchSurface.Indoor;
    public const string VenueMapsUrl = "https://maps.google.com/?q=Pabell%C3%B3n+Municipal";
    public const string ColorPrimary = "Blue";
    public const string ColorSecondary = "White";
    public const string ContactEmail = "info@fairplayfc.example";
    public const string ContactPhone = "+34 600 000 000";
    public const string Website = "https://fairplayfc.example";

    public const string CrestContentType = "image/png";
    public static byte[] CrestBytes => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static Venue Venue() => new(VenueName, VenueAddress, VenueSurface, VenueMapsUrl);

    public static KitColors Colors() => new(ColorPrimary, ColorSecondary);

    public static TeamProfile Profile() =>
        new(ShortName, FoundedYear, Venue(), Colors(), ContactEmail, ContactPhone, Website);

    public static CreateTeamCommand Command() =>
        new(Name, Coach, City, DefaultType, DefaultDivision, DefaultCategory);

    public static CreateTeamCommand CommandWithProfile() =>
        new(
            Name, Coach, City, DefaultType, DefaultDivision, DefaultCategory,
            ShortName, FoundedYear,
            VenueName, VenueAddress, VenueSurface, VenueMapsUrl,
            ColorPrimary, ColorSecondary,
            ContactEmail, ContactPhone, Website);

    public static UpdateTeamCommand UpdateCommand(Guid id) =>
        new(
            id, Name, Coach, City, DefaultType, DefaultDivision, DefaultCategory,
            ShortName, FoundedYear,
            VenueName, VenueAddress, VenueSurface, VenueMapsUrl,
            ColorPrimary, ColorSecondary,
            ContactEmail, ContactPhone, Website);

    public static Team DomainTeam(
        Guid? id = null,
        string? name = null,
        string? coach = null,
        string? city = null,
        FootballType? type = null,
        Division? division = null,
        AgeCategory? category = null,
        bool active = true,
        DateTime? createdAtUtc = null,
        TeamProfile? profile = null)
    {
        var team = Team.Create(
            id ?? Guid.NewGuid(),
            name ?? Name,
            coach ?? Coach,
            city ?? City,
            new TeamClassification(
                type ?? DefaultType,
                division ?? DefaultDivision,
                category ?? DefaultCategory),
            createdAtUtc ?? DateTime.UtcNow,
            profile);

        if (active)
            team.Activate();

        return team;
    }

    public static Team DomainTeamWithProfile(Guid? id = null) => DomainTeam(id, profile: Profile());

    public static Team DomainTeamWithCrest(Guid? id = null)
    {
        var team = DomainTeam(id);
        team.SetCrest(CrestBytes, CrestContentType);
        return team;
    }

    public static TeamDto Dto(Guid? id = null, bool hasCrest = false) =>
        new(
            id ?? Guid.NewGuid(),
            Name,
            Coach,
            City,
            DefaultType,
            DefaultDivision,
            DefaultCategory,
            hasCrest,
            DateTime.UtcNow,
            Active: true,
            ShortName: null,
            FoundedYear: null,
            VenueName: null,
            VenueAddress: null,
            VenueSurface: null,
            VenueMapsUrl: null,
            ColorPrimary: null,
            ColorSecondary: null,
            ContactEmail: null,
            ContactPhone: null,
            Website: null);

    public static string NameAlreadyExists => $"Team '{Name}' already exists.";
}
