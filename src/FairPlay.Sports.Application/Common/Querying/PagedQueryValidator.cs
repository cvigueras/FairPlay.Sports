using FluentValidation;

namespace FairPlay.Sports.Application.Common.Querying;

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
