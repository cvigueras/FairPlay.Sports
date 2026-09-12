using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Teams;

public interface ITeamRepository
{
    Task<PagedResult<Team>> GetPageAsync(
        IQueryFilter<Team> filter,
        IQuerySort<Team> sort,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Batch lookup for cross-slice DTO enrichment (e.g. Standings resolving team names).</summary>
    Task<IReadOnlyList<Team>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ids of teams matching the given classification (each null facet is unfiltered). Lets a
    /// cross-slice query - e.g. Standings filtering by a team's type/division/category - resolve
    /// to team ids without Standing itself carrying a navigation to Team.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetIdsByClassificationAsync(
        FootballType? type,
        Division? division,
        AgeCategory? category,
        CancellationToken cancellationToken = default);

    Task<Team?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<TeamCrest?> GetCrestAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Team team, CancellationToken cancellationToken = default);
}
