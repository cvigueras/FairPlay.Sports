using FairPlay.Sports.Application.Auth;
using FairPlay.Sports.Domain.Auth;
using FairPlay.Sports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Auth;

/// <summary>
/// Driven adapter: <see cref="IRefreshTokenRepository"/> over SQL Server. Reads are
/// deliberately tracked - the login/refresh/logout use cases mutate the returned tokens
/// (<c>Revoke</c>) and rely on a commit to persist them - so they skip <c>AsNoTracking()</c>.
/// Like every repository here it never calls <c>SaveChanges</c> itself.
/// </summary>
internal sealed class EfRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly FairPlaySportsDbContext _context;

    public EfRefreshTokenRepository(FairPlaySportsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default) =>
        await _context.RefreshTokens.AddAsync(token, cancellationToken);

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        _context.RefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(
        Guid userId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default) =>
        await _context.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAtUtc == null && token.ExpiresAtUtc > nowUtc)
            .ToListAsync(cancellationToken);
}
