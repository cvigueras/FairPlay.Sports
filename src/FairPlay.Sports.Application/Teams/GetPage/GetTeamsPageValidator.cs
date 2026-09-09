using FairPlay.Sports.Application.Common.Querying;
using FluentValidation;

namespace FairPlay.Sports.Application.Teams.GetPage;

public sealed class GetTeamsPageValidator : PagedQueryValidator<GetTeamsPageQuery>
{
    public GetTeamsPageValidator()
    {
        RuleFor(query => query.Sort)
            .Must(EveryFieldIsSortable)
            .WithMessage($"Sort accepts only: {string.Join(", ", TeamSort.AllowedFields)} (optionally '-' prefixed).");
    }

    private static bool EveryFieldIsSortable(string? sort) =>
        SortSpec.Parse(sort).All(field => TeamSort.AllowedFields.Contains(field.Field));
}
