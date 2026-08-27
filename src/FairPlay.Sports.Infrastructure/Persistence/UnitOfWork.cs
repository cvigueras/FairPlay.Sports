using FairPlay.Sports.Application.Common;

namespace FairPlay.Sports.Infrastructure.Persistence;

/// <summary>Driven adapter: commits the EF Core change tracker as one transaction.</summary>
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly FairPlaySportsDbContext _context;

    public UnitOfWork(FairPlaySportsDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
