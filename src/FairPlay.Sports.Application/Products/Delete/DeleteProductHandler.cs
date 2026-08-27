using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Products.Delete;

public sealed class DeleteProductHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IProductRepository _repository;

    public DeleteProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(request.Id, cancellationToken);

        return deleted
            ? Result.Success()
            : Result.NotFound($"Product '{request.Id}' was not found.");
    }
}
