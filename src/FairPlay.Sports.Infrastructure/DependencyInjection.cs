using FairPlay.Sports.Application.Auth;
using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FairPlay.Sports.Application.Users;
using FairPlay.Sports.Infrastructure.Auth;
using FairPlay.Sports.Infrastructure.Persistence;
using FairPlay.Sports.Infrastructure.Security;
using FairPlay.Sports.Infrastructure.Teams;
using FairPlay.Sports.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FairPlay.Sports.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Users slice: real PostgreSQL persistence via EF Core (Npgsql).
        var connectionString = configuration.GetConnectionString("FairPlaySports")
            ?? throw new InvalidOperationException(
                "Connection string 'FairPlaySports' was not found. Set ConnectionStrings:FairPlaySports.");

        services.AddDbContext<FairPlaySportsDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(FairPlaySportsDbContext).Assembly.FullName)));

        // DbContext, repositories and unit of work are all scoped (one per request).
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<ITeamRepository, EfTeamRepository>();
        services.AddScoped<IRefreshTokenRepository, EfRefreshTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Stateless, safe to share.
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer is required.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Audience is required.")
            .Validate(o => o.SigningKey is { Length: >= 32 }, "Jwt:SigningKey must be at least 32 characters.")
            .Validate(o => o.AccessTokenMinutes is > 0 and <= 1440, "Jwt:AccessTokenMinutes must be 1-1440.");

        return services;
    }
}
