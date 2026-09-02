using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Teams;

public sealed record TeamDto(
    Guid Id,
    string Name,
    string Coach,
    string City,
    FootballType Type,
    Division Division,
    AgeCategory Category,
    bool HasCrest,
    DateTime CreatedAt,
    bool Active)
{
    public static TeamDto FromDomain(Team team) =>
        new(
            team.Id,
            team.Name,
            team.Coach,
            team.City,
            team.Classification.Type,
            team.Classification.Division,
            team.Classification.Category,
            team.HasCrest,
            team.CreatedAt,
            team.Active);
}
