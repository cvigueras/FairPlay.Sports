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

    /// <summary>
    /// Whether a team with this name already exists in the same modality, division and category -
    /// the same name is fine across different classifications, and different clubs in the same
    /// classification tell themselves apart with a suffix (e.g. "Real Madrid A" / "Real Madrid B").
    /// <paramref name="excludeTeamId"/> excludes the team being updated from its own check.
    /// </summary>
    Task<bool> ExistsByNameAsync(
        string name,
        FootballType type,
        Division? division,
        AgeCategory category,
        Guid? excludeTeamId = null,
        CancellationToken cancellationToken = default);

    Task<TeamCrest?> GetCrestAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Team team, CancellationToken cancellationToken = default);
}
