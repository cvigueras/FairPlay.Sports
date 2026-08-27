using FluentValidation;
using MediatR;

namespace FairPlay.Sports.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs every FluentValidation validator registered for the
/// incoming request before it reaches its handler. This keeps validation as a cross-cutting
/// concern (one place, applied to every command/query) instead of being repeated inside each
/// handler. On failure it short-circuits the pipeline and returns a failed
/// <see cref="Result"/> / <see cref="Result{T}"/> - the same channel handlers use - rather than
/// throwing, so the API layer keeps translating outcomes uniformly.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(
                _validators.Select(validator => validator.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var error = string.Join("; ", failures.Select(failure => failure.ErrorMessage));
        return ResultFactory.Failure<TResponse>(error, ResultErrorType.Validation);
    }
}
