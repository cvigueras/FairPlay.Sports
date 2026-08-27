using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Products.Delete;

public sealed record DeleteProductCommand(Guid Id) : IRequest<Result>;
