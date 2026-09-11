using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Application.Standings;
using FairPlay.Sports.Domain.Standings;
using FairPlay.Sports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Standings;

internal sealed class EfStandingRepository : IStandingRepository
{
    private readonly FairPlaySportsDbContext _context;

    public EfStandingRepository(FairPlaySportsDbContext context)
    {
        _context = context;
    }

    public Task<PagedResult<Standing>> GetPageAsync(
        IQueryFilter<Standing> filter,
        IQuerySort<Standing> sort,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = sort.Apply(filter.Apply(_context.Standings.AsNoTracking()));
        return query.ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public Task<Standing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Standings
            .AsNoTracking()
            .FirstOrDefaultAsync(standing => standing.Id == id, cancellationToken);

    public Task<Standing?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Standings.FirstOrDefaultAsync(standing => standing.Id == id, cancellationToken);

    public Task<bool> ExistsByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default) =>
        _context.Standings.AnyAsync(standing => standing.TeamId == teamId, cancellationToken);

    public async Task AddAsync(Standing standing, CancellationToken cancellationToken = default) =>
        await _context.Standings.AddAsync(standing, cancellationToken);

    public Task RemoveAsync(Standing standing, CancellationToken cancellationToken = default)
    {
        _context.Standings.Remove(standing);
        return Task.CompletedTask;
    }
}
