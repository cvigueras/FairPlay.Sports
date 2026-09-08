using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Domain.Users;
using FairPlay.Sports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Users;

/// <summary>Driven adapter: implements <see cref="IUserRepository"/> over SQL Server.</summary>
internal sealed class EfUserRepository : IUserRepository
{
    private readonly FairPlaySportsDbContext _context;

    public EfUserRepository(FairPlaySportsDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Users
            .AsNoTracking()
            .OrderBy(user => user.UserName)
            .ToListAsync(cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Users.AnyAsync(user => user.Email == email, cancellationToken);

    public Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default) =>
        _context.Users.AnyAsync(user => user.UserName == userName, cancellationToken);

    public Task<UserPhoto?> GetPhotoAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Users
            .AsNoTracking()
            .Where(user => user.Id == id && user.Photo != null && user.PhotoContentType != null)
            .Select(user => new UserPhoto(user.Photo!, user.PhotoContentType!))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await _context.Users.AddAsync(user, cancellationToken);
}
