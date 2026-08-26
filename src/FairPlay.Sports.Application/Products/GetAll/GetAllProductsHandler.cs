using FairPlay.Sports.Application.Abstractions;
using FairPlay.Sports.Application.Common;

namespace FairPlay.Sports.Application.Products.GetAll;

public sealed class GetAllProductsHandler : IQueryHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductRepository _repository;

    public GetAllProductsHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> Handle(GetAllProductsQuery query, CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetAllAsync(cancellationToken);
        IReadOnlyList<ProductDto> dtos = products.Select(ProductDto.FromDomain).ToList();

        return Result<IReadOnlyList<ProductDto>>.Success(dtos);
    }
}
