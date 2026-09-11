using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Teams;

internal sealed class EfTeamRepository : ITeamRepository
{
    private readonly FairPlaySportsDbContext _context;

    public EfTeamRepository(FairPlaySportsDbContext context)
    {
        _context = context;
    }

    public Task<PagedResult<Team>> GetPageAsync(
        IQueryFilter<Team> filter,
        IQuerySort<Team> sort,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = sort.Apply(filter.Apply(_context.Teams.AsNoTracking()));
        return query.ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Teams
            .AsNoTracking()
            .FirstOrDefaultAsync(team => team.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Team>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids as ICollection<Guid> ?? ids.ToList();
        if (idList.Count == 0)
            return [];

        return await _context.Teams
            .AsNoTracking()
            .Where(team => idList.Contains(team.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<Team?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Teams.FirstOrDefaultAsync(team => team.Id == id, cancellationToken);

    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Teams.AnyAsync(team => team.Id == id, cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default) =>
        _context.Teams.AnyAsync(team => team.Name == name, cancellationToken);

    public Task<TeamCrest?> GetCrestAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Teams
            .AsNoTracking()
            .Where(team => team.Id == id && team.Crest != null && team.CrestContentType != null)
            .Select(team => new TeamCrest(team.Crest!, team.CrestContentType!))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(Team team, CancellationToken cancellationToken = default) =>
        await _context.Teams.AddAsync(team, cancellationToken);
}
