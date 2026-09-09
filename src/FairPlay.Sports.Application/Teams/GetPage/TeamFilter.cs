using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Teams.GetPage;

/// <summary>
/// The fields <c>GET /api/Teams</c> can be filtered by. Every property is
/// optional; a null one adds no restriction. Name/City match case-insensitively
/// as "contains"; the rest are exact.
/// </summary>
public sealed record TeamFilter(
    string? Name = null,
    string? City = null,
    FootballType? Type = null,
    Division? Division = null,
    AgeCategory? Category = null,
    bool? Active = null) : IQueryFilter<Team>
{
    public IQueryable<Team> Apply(IQueryable<Team> source)
    {
        var query = source;

        if (!string.IsNullOrWhiteSpace(Name))
        {
            var name = Name.Trim().ToLower();
            query = query.Where(team => team.Name.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(City))
        {
            var city = City.Trim().ToLower();
            query = query.Where(team => team.City.ToLower().Contains(city));
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
