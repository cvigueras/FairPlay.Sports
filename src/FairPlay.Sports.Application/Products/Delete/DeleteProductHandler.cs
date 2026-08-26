using FairPlay.Sports.Application.Abstractions;
using FairPlay.Sports.Application.Common;

namespace FairPlay.Sports.Application.Products.Delete;

public sealed class DeleteProductHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly IProductRepository _repository;

    public DeleteProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteProductCommand command, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(command.Id, cancellationToken);

        return deleted
            ? Result.Success()
            : Result.NotFound($"Product '{command.Id}' was not found.");
    }
}
