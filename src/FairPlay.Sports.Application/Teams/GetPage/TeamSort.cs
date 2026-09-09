using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Teams.GetPage;

public sealed class TeamSort(string? sort) : IQuerySort<Team>
{
    public static readonly IReadOnlySet<string> AllowedFields =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "name", "city", "createdAt", "active", "type" };

    private readonly IReadOnlyList<SortField> _fields = SortSpec.Parse(sort);

    public IQueryable<Team> Apply(IQueryable<Team> source)
    {
        IOrderedQueryable<Team>? ordered = null;

        foreach (var field in _fields)
        {
            ordered = field.Field switch
            {
                "name" => Chain(source, ordered, team => team.Name, field.Descending),
                "city" => Chain(source, ordered, team => team.City, field.Descending),
                "createdat" => Chain(source, ordered, team => team.CreatedAt, field.Descending),
                "active" => Chain(source, ordered, team => team.Active, field.Descending),
                "type" => Chain(source, ordered, team => team.Classification.Type, field.Descending),
                _ => ordered
            };
        }

        return ordered is null
            ? source.OrderBy(team => team.Name)
            : ordered.ThenBy(team => team.Name);
    }

    private static IOrderedQueryable<Team> Chain<TKey>(
        IQueryable<Team> source,
        IOrderedQueryable<Team>? ordered,
        System.Linq.Expressions.Expression<Func<Team, TKey>> keySelector,
        bool descending)
    {
        if (ordered is null)
        {
            return descending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector);
        }

        return descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
    }
}
