namespace FairPlay.Sports.Application.Common;

/// <summary>
/// Driven port: commits the changes tracked during a single use case as one atomic
/// unit. Implemented in Infrastructure by the EF Core <c>DbContext</c>. Handlers add or
/// mutate aggregates through their repositories and never call this directly - the
/// <see cref="Behaviors.UnitOfWorkBehavior{TRequest,TResponse}"/> commits once per
/// successful command.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
