using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Products;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Infrastructure.Persistence;
using FairPlay.Sports.Infrastructure.Products;
using FairPlay.Sports.Infrastructure.Security;
using FairPlay.Sports.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FairPlay.Sports.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Products slice: kept on the seeded in-memory adapter (demo only, no database).
        // Singleton so create/update/delete survive for the lifetime of the process.
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();

        // Users slice: real SQL Server persistence via EF Core.
        var connectionString = configuration.GetConnectionString("FairPlaySports")
            ?? throw new InvalidOperationException(
                "Connection string 'FairPlaySports' was not found. Set ConnectionStrings:FairPlaySports.");

        services.AddDbContext<FairPlaySportsDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(FairPlaySportsDbContext).Assembly.FullName)));

        // DbContext, repository and unit of work are all scoped (one per request).
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Stateless, safe to share.
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
