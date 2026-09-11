using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Domain.Standings;

namespace FairPlay.Sports.Application.Standings.GetPage;

public sealed class StandingSort(string? sort) : IQuerySort<Standing>
{
    public static readonly IReadOnlySet<string> AllowedFields =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "points", "played", "won", "drawn", "lost", "goalsfor", "goalsagainst", "goaldifference"
        };

    private readonly IReadOnlyList<SortField> _fields = SortSpec.Parse(sort);

    public IQueryable<Standing> Apply(IQueryable<Standing> source)
    {
        IOrderedQueryable<Standing>? ordered = null;

        foreach (var field in _fields)
        {
            ordered = field.Field switch
            {
                "points" => Chain(source, ordered, standing => standing.Points, field.Descending),
                "played" => Chain(source, ordered, standing => standing.Played, field.Descending),
                "won" => Chain(source, ordered, standing => standing.Won, field.Descending),
                "drawn" => Chain(source, ordered, standing => standing.Drawn, field.Descending),
                "lost" => Chain(source, ordered, standing => standing.Lost, field.Descending),
                "goalsfor" => Chain(source, ordered, standing => standing.GoalsFor, field.Descending),
                "goalsagainst" => Chain(source, ordered, standing => standing.GoalsAgainst, field.Descending),
                "goaldifference" => Chain(source, ordered, standing => standing.GoalsFor - standing.GoalsAgainst, field.Descending),
                _ => ordered
            };
        }

        // No sort requested: the natural default for a classification table is points, best first.
        return ordered is null
            ? source.OrderByDescending(standing => standing.Points).ThenBy(standing => standing.Id)
            : ordered.ThenBy(standing => standing.Id);
    }

    private static IOrderedQueryable<Standing> Chain<TKey>(
        IQueryable<Standing> source,
        IOrderedQueryable<Standing>? ordered,
        System.Linq.Expressions.Expression<Func<Standing, TKey>> keySelector,
        bool descending)
    {
        if (ordered is null)
        {
            return descending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector);
        }

        return descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
    }
}
