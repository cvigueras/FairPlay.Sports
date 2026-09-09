using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Teams.Create;
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

    public const string CrestContentType = "image/png";
    public static byte[] CrestBytes => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static CreateTeamCommand Command() =>
        new(Name, Coach, City, DefaultType, DefaultDivision, DefaultCategory);

    public static Team DomainTeam(
        Guid? id = null,
        string? name = null,
        string? coach = null,
        string? city = null,
        FootballType? type = null,
        Division? division = null,
        AgeCategory? category = null,
        bool active = true,
        DateTime? createdAtUtc = null)
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
            createdAtUtc ?? DateTime.UtcNow);

        if (active)
            team.Activate();

        return team;
    }

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
            Active: true);

    public static string NameAlreadyExists => $"Team '{Name}' already exists.";
}
