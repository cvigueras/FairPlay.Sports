using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Teams;

public interface ITeamRepository
{
    Task<IReadOnlyList<Team>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Team?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<TeamCrest?> GetCrestAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Team team, CancellationToken cancellationToken = default);
}
