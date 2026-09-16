using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Teams;

internal sealed class EfTeamMemberRepository : ITeamMemberRepository
{
    private readonly FairPlaySportsDbContext _context;

    public EfTeamMemberRepository(FairPlaySportsDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TeamMember>> GetByTeamIdAsync(
        Guid teamId, CancellationToken cancellationToken = default) =>
        await _context.TeamMembers
            .AsNoTracking()
            .Where(member => member.TeamId == teamId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TeamMember>> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default) =>
        await _context.TeamMembers
            .AsNoTracking()
            .Where(member => member.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task<TeamMember?> GetByTeamAndUserForUpdateAsync(
        Guid teamId, Guid userId, CancellationToken cancellationToken = default) =>
        _context.TeamMembers
            .FirstOrDefaultAsync(member => member.TeamId == teamId && member.UserId == userId, cancellationToken);

    public Task<bool> ExistsForUserAndTeamAsync(
        Guid teamId, Guid userId, CancellationToken cancellationToken = default) =>
        _context.TeamMembers.AnyAsync(member => member.TeamId == teamId && member.UserId == userId, cancellationToken);

    public Task<bool> ExistsForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _context.TeamMembers.AnyAsync(member => member.UserId == userId, cancellationToken);

    public Task<bool> ExistsWithRoleAsync(
        Guid teamId, TeamMemberRole role, CancellationToken cancellationToken = default) =>
        _context.TeamMembers.AnyAsync(member => member.TeamId == teamId && member.Role == role, cancellationToken);

    public async Task AddAsync(TeamMember member, CancellationToken cancellationToken = default) =>
        await _context.TeamMembers.AddAsync(member, cancellationToken);

    public void Remove(TeamMember member) => _context.TeamMembers.Remove(member);
}
