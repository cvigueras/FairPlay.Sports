using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Teams;

/// <summary>
/// Driven port for team persistence. Writes are not flushed here; the
/// <see cref="Common.Behaviors.UnitOfWorkBehavior{TRequest,TResponse}"/> commits the
/// unit of work once the command succeeds.
/// </summary>
public interface ITeamRepository
{
    Task<IReadOnlyList<Team>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Read-only lookup (not tracked).</summary>
    Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Tracked lookup for use cases that mutate the team and rely on a commit.</summary>
    Task<Team?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Projects just the crest columns; null when the team is missing or has no crest.</summary>
    Task<TeamCrest?> GetCrestAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Team team, CancellationToken cancellationToken = default);
}
