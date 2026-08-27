using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Products.Create;

public sealed record CreateProductCommand(string Name, string Description, decimal Price, int Stock)
    : IRequest<Result<ProductDto>>;
