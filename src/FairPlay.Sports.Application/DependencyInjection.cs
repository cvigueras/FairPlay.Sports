using FairPlay.Sports.Application.Abstractions;
using FairPlay.Sports.Application.Products;
using FairPlay.Sports.Application.Products.Create;
using FairPlay.Sports.Application.Products.Delete;
using FairPlay.Sports.Application.Products.GetAll;
using FairPlay.Sports.Application.Products.GetById;
using FairPlay.Sports.Application.Products.Update;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FairPlay.Sports.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

        services.AddScoped<ICommandHandler<CreateProductCommand, ProductDto>, CreateProductHandler>();
        services.AddScoped<ICommandHandler<UpdateProductCommand, ProductDto>, UpdateProductHandler>();
        services.AddScoped<ICommandHandler<DeleteProductCommand>, DeleteProductHandler>();
        services.AddScoped<IQueryHandler<GetProductByIdQuery, ProductDto>, GetProductByIdHandler>();
        services.AddScoped<IQueryHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>>, GetAllProductsHandler>();

        return services;
    }
}
