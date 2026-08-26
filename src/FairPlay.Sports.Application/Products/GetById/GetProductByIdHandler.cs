using FairPlay.Sports.Application.Abstractions;
using FairPlay.Sports.Application.Common;

namespace FairPlay.Sports.Application.Products.GetById;

public sealed class GetProductByIdHandler : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductRepository _repository;

    public GetProductByIdHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(query.Id, cancellationToken);

        return product is null
            ? Result<ProductDto>.NotFound($"Product '{query.Id}' was not found.")
            : Result<ProductDto>.Success(ProductDto.FromDomain(product));
    }
}
