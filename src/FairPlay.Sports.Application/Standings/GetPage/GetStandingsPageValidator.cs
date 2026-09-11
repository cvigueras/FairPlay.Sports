using FairPlay.Sports.Application.Common.Querying;
using FluentValidation;

namespace FairPlay.Sports.Application.Standings.GetPage;

public sealed class GetStandingsPageValidator : PagedQueryValidator<GetStandingsPageQuery>
{
    public GetStandingsPageValidator()
    {
        RuleFor(query => query.Sort)
            .Must(EveryFieldIsSortable)
            .WithMessage($"Sort accepts only: {string.Join(", ", StandingSort.AllowedFields)} (optionally '-' prefixed).");
    }

    private static bool EveryFieldIsSortable(string? sort) =>
        SortSpec.Parse(sort).All(field => StandingSort.AllowedFields.Contains(field.Field));
}
