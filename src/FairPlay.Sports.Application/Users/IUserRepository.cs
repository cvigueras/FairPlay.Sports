using FairPlay.Sports.Domain.Users;

namespace FairPlay.Sports.Application.Users;

/// <summary>
/// Driven port for user persistence. Writes are not flushed here; the
/// <see cref="Common.Behaviors.UnitOfWorkBehavior{TRequest,TResponse}"/> commits the
/// unit of work once the command succeeds.
/// </summary>
public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Tracked lookup for use cases that mutate the user and rely on a commit.</summary>
    Task<User?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    Task<UserPhoto?> GetPhotoAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
