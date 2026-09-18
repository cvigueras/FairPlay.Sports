using FairPlay.Sports.Application.Challenges;
using FairPlay.Sports.Domain.Challenges;
using FairPlay.Sports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Challenges;

internal sealed class EfChallengeRepository : IChallengeRepository
{
    private readonly FairPlaySportsDbContext _context;

    public EfChallengeRepository(FairPlaySportsDbContext context)
    {
        _context = context;
    }

    public Task<Challenge?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Challenges.AsNoTracking().FirstOrDefaultAsync(challenge => challenge.Id == id, cancellationToken);

    public Task<Challenge?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Challenges.FirstOrDefaultAsync(challenge => challenge.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Challenge>> GetByTeamIdAsync(
        Guid teamId, CancellationToken cancellationToken = default) =>
        await _context.Challenges
            .AsNoTracking()
            .Where(challenge => challenge.ChallengerTeamId == teamId || challenge.ChallengedTeamId == teamId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Challenge challenge, CancellationToken cancellationToken = default) =>
        await _context.Challenges.AddAsync(challenge, cancellationToken);
}
