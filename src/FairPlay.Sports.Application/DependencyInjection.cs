using FairPlay.Sports.Application.Common.Behaviors;
using FairPlay.Sports.Application.Products.Create;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FairPlay.Sports.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR scans this assembly for IRequestHandler<,> implementations (one per vertical slice)
        // and wires the ValidationBehavior into every request pipeline.
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblyContaining<CreateProductHandler>();
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

        return services;
    }
}
