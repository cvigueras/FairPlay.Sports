using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Products.GetById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;
