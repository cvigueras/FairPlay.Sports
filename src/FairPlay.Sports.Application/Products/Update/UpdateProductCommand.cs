using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Products.Update;

public sealed record UpdateProductCommand(Guid Id, string Name, string Description, decimal Price, int Stock)
    : IRequest<Result<ProductDto>>;
