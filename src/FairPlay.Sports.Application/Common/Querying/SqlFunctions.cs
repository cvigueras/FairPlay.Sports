using System.Globalization;
using System.Text;

namespace FairPlay.Sports.Application.Common.Querying;

/// <summary>
/// Helpers that a query filter can call inside an <see cref="IQueryable{T}"/> expression.
/// Infrastructure maps <see cref="Unaccent"/> onto PostgreSQL's <c>unaccent()</c> so the
/// database does the work; the C# body below is the equivalent fallback used when the
/// expression runs in memory (unit tests, LINQ-to-Objects).
/// </summary>
public static class SqlFunctions
{
    public static string Unaccent(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
