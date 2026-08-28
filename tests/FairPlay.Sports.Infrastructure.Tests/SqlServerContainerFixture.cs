using FairPlay.Sports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace FairPlay.Sports.Infrastructure.Tests;

/// <summary>
/// Starts one SQL Server container for the whole test assembly and applies every EF Core
/// migration to it once. Fixtures read <see cref="ConnectionString"/> to open their own
/// <see cref="FairPlaySportsDbContext"/> instances; keeping rows isolated between tests is
/// each fixture's own job (see <see cref="RepositoryTestBase"/>).
/// </summary>
/// <remarks>
/// Locally the container is <b>reused</b> across test runs (not torn down) so you only pay the
/// SQL Server start-up cost once - enable it globally with <c>testcontainers.reuse.enable=true</c>
/// in <c>~/.testcontainers.properties</c>. On CI (env var <c>CI</c> set) reuse is off and the
/// container is created fresh and removed at the end of the run.
/// </remarks>
[SetUpFixture]
public sealed class SqlServerContainerFixture
{
    private static readonly bool ReuseContainer =
        Environment.GetEnvironmentVariable("CI") is null;

    private static readonly MsSqlContainer Container =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .WithReuse(ReuseContainer)
            .Build();

    public static string ConnectionString { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task StartContainerAndMigrate()
    {
        await Container.StartAsync();
        ConnectionString = Container.GetConnectionString();

        await using var context = new FairPlaySportsDbContext(
            new DbContextOptionsBuilder<FairPlaySportsDbContext>()
                .UseSqlServer(ConnectionString)
                .Options);

        // No-op on a reused container that is already migrated.
        await context.Database.MigrateAsync();
    }

    [OneTimeTearDown]
    public async Task StopContainer()
    {
        if (!ReuseContainer)
            await Container.DisposeAsync();
    }
}
