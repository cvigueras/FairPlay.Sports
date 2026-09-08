using FairPlay.Sports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FairPlay.Sports.Infrastructure.Tests;

/// <summary>
/// Base for tests that hit the real PostgreSQL container. Tests build fresh
/// <see cref="FairPlaySportsDbContext"/> instances through <see cref="NewContext"/> (a separate
/// one for arrange, act and assert avoids false positives from the change tracker); the
/// tables are emptied after every test so cases stay isolated.
/// </summary>
public abstract class RepositoryTestBase
{
    protected static FairPlaySportsDbContext NewContext() =>
        new(new DbContextOptionsBuilder<FairPlaySportsDbContext>()
            .UseNpgsql(PostgreSqlContainerFixture.ConnectionString)
            .Options);

    [TearDown]
    public async Task EmptyTables()
    {
        await using var context = NewContext();
        // RefreshTokens first: it has an FK to Users.
        await context.RefreshTokens.ExecuteDeleteAsync();
        await context.Users.ExecuteDeleteAsync();
        await context.Teams.ExecuteDeleteAsync();
    }
}
