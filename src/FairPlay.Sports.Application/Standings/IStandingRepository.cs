using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Domain.Standings;

namespace FairPlay.Sports.Application.Standings;

public interface IStandingRepository
{
    Task<PagedResult<Standing>> GetPageAsync(
        IQueryFilter<Standing> filter,
        IQuerySort<Standing> sort,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Standing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Standing?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);

    Task AddAsync(Standing standing, CancellationToken cancellationToken = default);

    Task RemoveAsync(Standing standing, CancellationToken cancellationToken = default);
}
