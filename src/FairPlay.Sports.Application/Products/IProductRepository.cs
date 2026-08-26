using FairPlay.Sports.Domain.Products;

namespace FairPlay.Sports.Application.Products;

/// <summary>
/// Driven port: defines what the application needs from persistence,
/// without knowing anything about how it is actually implemented (hexagonal architecture).
/// </summary>
public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
