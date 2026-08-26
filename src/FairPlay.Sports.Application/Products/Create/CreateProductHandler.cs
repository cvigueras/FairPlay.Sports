using FairPlay.Sports.Application.Abstractions;
using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Domain.Products;
using FluentValidation;

namespace FairPlay.Sports.Application.Products.Create;

public sealed class CreateProductHandler : ICommandHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _repository;
    private readonly IValidator<CreateProductCommand> _validator;

    public CreateProductHandler(IProductRepository repository, IValidator<CreateProductCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            var error = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return Result<ProductDto>.Failure(error);
        }

        var product = new Product(Guid.NewGuid(), command.Name, command.Description, command.Price, command.Stock);
        await _repository.AddAsync(product, cancellationToken);

        return Result<ProductDto>.Success(ProductDto.FromDomain(product));
    }
}
