using FairPlay.Sports.Domain.Challenges;

namespace FairPlay.Sports.Application.Challenges;

public interface IChallengeRepository
{
    Task<Challenge?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Challenge?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Every challenge a team sent or received.</summary>
    Task<IReadOnlyList<Challenge>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);

    Task AddAsync(Challenge challenge, CancellationToken cancellationToken = default);
}
