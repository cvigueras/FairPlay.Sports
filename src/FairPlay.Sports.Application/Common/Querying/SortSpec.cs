namespace FairPlay.Sports.Application.Common.Querying;

/// <summary>One requested sort key. <paramref name="Field"/> is lower-cased.</summary>
public readonly record struct SortField(string Field, bool Descending);

/// <summary>
/// Parses the <see cref="IPagedQuery.Sort"/> string into ordered <see cref="SortField"/>s.
/// Format: comma-separated field names, an optional leading <c>-</c> for descending
/// (a leading <c>+</c> is allowed and means ascending). Blanks are ignored.
/// </summary>
public static class SortSpec
{
    public static IReadOnlyList<SortField> Parse(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return [];
        }

        var fields = new List<SortField>();

        foreach (var token in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var descending = token[0] == '-';
            var name = token[0] is '-' or '+' ? token[1..].Trim() : token;

            if (name.Length > 0)
            {
                fields.Add(new SortField(name.ToLowerInvariant(), descending));
            }
        }

        return fields;
    }
}
