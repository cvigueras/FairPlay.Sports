using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Products.Update;

public sealed class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IProductRepository _repository;

    public UpdateProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return Result<ProductDto>.NotFound($"Product '{request.Id}' was not found.");
        }

        product.UpdateDetails(request.Name, request.Description, request.Price, request.Stock);
        await _repository.UpdateAsync(product, cancellationToken);

        return Result<ProductDto>.Success(ProductDto.FromDomain(product));
    }
}
