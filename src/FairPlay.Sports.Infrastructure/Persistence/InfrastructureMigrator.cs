using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FairPlay.Sports.Infrastructure.Persistence;

public static class InfrastructureMigrator
{
    /// <summary>
    /// Applies every pending EF Core migration to the configured database. Intended to
    /// be called from the host on startup <b>only in local/dev environments</b> - it
    /// takes a schema lock and is not safe to run from several instances at once, so
    /// production should apply migrations as an explicit deploy step instead.
    /// </summary>
    public static async Task MigrateAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FairPlaySportsDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }
}
