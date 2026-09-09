using FluentValidation;

namespace FairPlay.Sports.Application.Common.Querying;

/// <summary>
/// Reusable base validator for any paged query. Slice validators inherit it and
/// add their own field-level rules (for example the accepted sort fields). It is
/// discovered by <c>AddValidatorsFromAssemblyContaining</c> through the concrete
/// subclass, so the page/size bounds are enforced everywhere for free.
/// </summary>
public abstract class PagedQueryValidator<TQuery> : AbstractValidator<TQuery>
    where TQuery : IPagedQuery
{
    protected PagedQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, PaginationDefaults.MaxPageSize);
    }
}
