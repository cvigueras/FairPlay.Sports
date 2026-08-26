using FairPlay.Sports.Application.Products;
using FairPlay.Sports.Infrastructure.Products;
using Microsoft.Extensions.DependencyInjection;

namespace FairPlay.Sports.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Singleton: keeps the dummy in-memory data set alive (with any create/update/delete)
        // for the lifetime of the running application.
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();

        return services;
    }
}
