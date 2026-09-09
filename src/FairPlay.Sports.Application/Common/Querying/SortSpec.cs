namespace FairPlay.Sports.Application.Common.Querying;

public readonly record struct SortField(string Field, bool Descending);

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
