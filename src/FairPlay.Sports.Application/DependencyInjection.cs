using FairPlay.Sports.Application.Auth;
using FairPlay.Sports.Application.Common.Behaviors;
using FairPlay.Sports.Application.Users.Register;
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
            configuration.RegisterServicesFromAssemblyContaining<RegisterUserHandler>();
            // Order matters: validation runs first, then the unit of work commits a
            // successful command inside it.
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });

        services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();

        // Application service shared by the login and refresh handlers.
        services.AddScoped<IAuthTokenIssuer, AuthTokenIssuer>();

        return services;
    }
}
