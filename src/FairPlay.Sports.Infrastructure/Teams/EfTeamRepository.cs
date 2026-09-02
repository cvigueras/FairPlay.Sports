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

    public async Task<IReadOnlyList<Team>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Teams
            .AsNoTracking()
            .OrderBy(team => team.Name)
            .ToListAsync(cancellationToken);

    public Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Teams
            .AsNoTracking()
            .FirstOrDefaultAsync(team => team.Id == id, cancellationToken);

    public Task<Team?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Teams.FirstOrDefaultAsync(team => team.Id == id, cancellationToken);

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
