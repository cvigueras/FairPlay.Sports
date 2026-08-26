using FairPlay.Sports.Domain.Products;

namespace FairPlay.Sports.Application.Products;

public sealed record ProductDto(Guid Id, string Name, string Description, decimal Price, int Stock)
{
    public static ProductDto FromDomain(Product product) =>
        new(product.Id, product.Name, product.Description, product.Price, product.Stock);
}
