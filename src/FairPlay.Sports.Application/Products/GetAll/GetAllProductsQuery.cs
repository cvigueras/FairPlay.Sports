using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Products.GetAll;

public sealed record GetAllProductsQuery : IRequest<Result<IReadOnlyList<ProductDto>>>;
