using MediatR;

namespace FairPlay.Sports.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that commits the unit of work exactly once, after a
/// command's handler has run successfully. This keeps persistence a cross-cutting
/// concern: handlers add or mutate aggregates through their repositories and stay
/// free of <c>SaveChanges</c> calls.
/// <para>
/// Only requests whose type name ends in <c>Command</c> trigger a commit - the
/// codebase convention for writes. Queries flow straight through. A failed
/// <see cref="IResult"/> also skips the commit, so an invalid or not-found outcome
/// never persists partial work. It runs inside
/// <see cref="ValidationBehavior{TRequest,TResponse}"/>, so validation failures never
/// reach it.
/// </para>
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        var isCommand = typeof(TRequest).Name.EndsWith("Command", StringComparison.Ordinal);
        if (isCommand && response is not IResult { IsSuccess: false })
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
