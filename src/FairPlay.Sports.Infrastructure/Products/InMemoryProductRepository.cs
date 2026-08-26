using System.Collections.Concurrent;
using FairPlay.Sports.Application.Products;
using FairPlay.Sports.Domain.Products;

namespace FairPlay.Sports.Infrastructure.Products;

/// <summary>
/// Driven adapter: implements the IProductRepository port with an in-memory, seeded
/// dummy data set. No real database is used - this is intentional for the demo.
/// </summary>
public sealed class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _products = new();

    public InMemoryProductRepository()
    {
        foreach (var product in CreateSeedData())
        {
            _products[product.Id] = product;
        }
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Product> result = _products.Values.OrderBy(p => p.Name).ToList();
        return Task.FromResult(result);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _products[product.Id] = product;
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (!_products.ContainsKey(product.Id))
        {
            return Task.FromResult(false);
        }

        _products[product.Id] = product;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_products.TryRemove(id, out _));
    }

    private static IEnumerable<Product> CreateSeedData()
    {
        yield return new Product(Guid.NewGuid(), "Balón de fútbol Pro", "Balón oficial talla 5, cosido a mano.", 29.99m, 120);
        yield return new Product(Guid.NewGuid(), "Zapatillas Running X200", "Zapatillas ligeras con amortiguación de gel.", 89.90m, 45);
        yield return new Product(Guid.NewGuid(), "Raqueta de pádel Carbon", "Raqueta de fibra de carbono, núcleo EVA.", 149.50m, 18);
        yield return new Product(Guid.NewGuid(), "Camiseta técnica DryFit", "Camiseta transpirable para entrenamiento.", 19.99m, 200);
        yield return new Product(Guid.NewGuid(), "Mancuernas ajustables 20kg", "Set de mancuernas ajustables de 2 a 20 kg.", 129.00m, 30);
    }
}
