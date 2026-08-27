using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Products.GetById;

public sealed class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IProductRepository _repository;

    public GetProductByIdHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);

        return product is null
            ? Result<ProductDto>.NotFound($"Product '{request.Id}' was not found.")
            : Result<ProductDto>.Success(ProductDto.FromDomain(product));
    }
}
