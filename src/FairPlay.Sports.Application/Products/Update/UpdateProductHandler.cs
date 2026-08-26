using FairPlay.Sports.Application.Abstractions;
using FairPlay.Sports.Application.Common;
using FluentValidation;

namespace FairPlay.Sports.Application.Products.Update;

public sealed class UpdateProductHandler : ICommandHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductRepository _repository;
    private readonly IValidator<UpdateProductCommand> _validator;

    public UpdateProductHandler(IProductRepository repository, IValidator<UpdateProductCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            var error = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return Result<ProductDto>.Failure(error);
        }

        var product = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
        {
            return Result<ProductDto>.NotFound($"Product '{command.Id}' was not found.");
        }

        product.UpdateDetails(command.Name, command.Description, command.Price, command.Stock);
        await _repository.UpdateAsync(product, cancellationToken);

        return Result<ProductDto>.Success(ProductDto.FromDomain(product));
    }
}
