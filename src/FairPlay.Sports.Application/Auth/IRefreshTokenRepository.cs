using FairPlay.Sports.Domain.Auth;

namespace FairPlay.Sports.Application.Auth;

/// <summary>
/// Driven port for refresh-token persistence. Unlike the read-only queries elsewhere,
/// <see cref="GetByTokenHashAsync"/> and <see cref="GetActiveByUserIdAsync"/> return
/// tracked aggregates: the login and refresh flows mutate them (<c>Revoke</c>) and the
/// <see cref="Common.Behaviors.UnitOfWorkBehavior{TRequest,TResponse}"/> commits.
/// </summary>
public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(
        Guid userId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);
}
