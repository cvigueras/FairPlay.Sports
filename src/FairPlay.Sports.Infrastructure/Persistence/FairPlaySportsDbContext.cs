using FairPlay.Sports.Application.Common.Querying;
using FairPlay.Sports.Domain.Auth;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the PostgreSQL database. It is also the unit of work:
/// <see cref="Persistence.UnitOfWork"/> delegates its commit to
/// <see cref="DbContext.SaveChangesAsync(CancellationToken)"/>.
/// </summary>
public sealed class FairPlaySportsDbContext : DbContext
{
    public FairPlaySportsDbContext(DbContextOptions<FairPlaySportsDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("unaccent");

        // Accent-insensitive text search: map SqlFunctions.Unaccent onto the extension's function.
        modelBuilder
            .HasDbFunction(typeof(SqlFunctions).GetMethod(nameof(SqlFunctions.Unaccent), [typeof(string)])!)
            .HasName("unaccent");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FairPlaySportsDbContext).Assembly);
    }
}
