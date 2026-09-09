using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Teams.GetPage;

public sealed record TeamFilter(
    string? Name = null,
    string? City = null,
    string? Coach = null,
    FootballType? Type = null,
    Division? Division = null,
    AgeCategory? Category = null,
    bool? Active = null) : IQueryFilter<Team>
{
    public IQueryable<Team> Apply(IQueryable<Team> source)
    {
        var query = source;

        // Text filters are contains, case-insensitive and accent-insensitive
        // (searching "Martinez" must find "Martínez").
        if (!string.IsNullOrWhiteSpace(Name))
        {
            var name = SqlFunctions.Unaccent(Name.Trim().ToLower());
            query = query.Where(team => SqlFunctions.Unaccent(team.Name.ToLower()).Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(City))
        {
            var city = SqlFunctions.Unaccent(City.Trim().ToLower());
            query = query.Where(team => SqlFunctions.Unaccent(team.City.ToLower()).Contains(city));
        }

        if (!string.IsNullOrWhiteSpace(Coach))
        {
            var coach = SqlFunctions.Unaccent(Coach.Trim().ToLower());
            query = query.Where(team => SqlFunctions.Unaccent(team.Coach.ToLower()).Contains(coach));
        }

        if (Type is not null)
        {
            query = query.Where(team => team.Classification.Type == Type);
        }

        if (Division is not null)
        {
            query = query.Where(team => team.Classification.Division == Division);
        }

        if (Category is not null)
        {
            query = query.Where(team => team.Classification.Category == Category);
        }

        if (Active is not null)
        {
            query = query.Where(team => team.Active == Active);
        }

        return query;
    }
}
