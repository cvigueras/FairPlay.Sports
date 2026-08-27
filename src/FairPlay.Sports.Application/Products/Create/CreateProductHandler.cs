using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Domain.Products;
using MediatR;

namespace FairPlay.Sports.Application.Products.Create;

public sealed class CreateProductHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IProductRepository _repository;

    public CreateProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken = default)
    {
        var product = new Product(Guid.NewGuid(), request.Name, request.Description, request.Price, request.Stock);
        await _repository.AddAsync(product, cancellationToken);

        return Result<ProductDto>.Success(ProductDto.FromDomain(product));
    }
}
