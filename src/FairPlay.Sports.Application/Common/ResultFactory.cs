using System.Reflection;

namespace FairPlay.Sports.Application.Common;

/// <summary>
/// Builds a failed <see cref="Result"/> / <see cref="Result{T}"/> for an arbitrary closed
/// response type known only at run time. Used by pipeline behaviors (for example
/// <see cref="Behaviors.ValidationBehavior{TRequest,TResponse}"/>) that must short-circuit the
/// MediatR pipeline and hand a failure back through the same Result channel the handlers use,
/// without knowing the concrete <c>TResponse</c> at compile time.
/// </summary>
internal static class ResultFactory
{
    public static TResponse Failure<TResponse>(string error, ResultErrorType errorType)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error, errorType);
        }

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = typeof(TResponse).GetGenericArguments()[0];
            var failureMethod = typeof(Result<>)
                .MakeGenericType(valueType)
                .GetMethod(
                    nameof(Result<object>.Failure),
                    BindingFlags.Public | BindingFlags.Static,
                    binder: null,
                    types: [typeof(string), typeof(ResultErrorType)],
                    modifiers: null)!;

            return (TResponse)failureMethod.Invoke(null, [error, errorType])!;
        }

        throw new InvalidOperationException(
            $"Cannot build a failure of type '{typeof(TResponse)}'. Requests routed through Result " +
            "pipeline behaviors must return Result or Result<T>.");
    }
}
