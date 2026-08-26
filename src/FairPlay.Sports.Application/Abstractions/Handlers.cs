using FairPlay.Sports.Application.Common;

namespace FairPlay.Sports.Application.Abstractions;

public interface ICommandHandler<in TCommand>
{
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand, TResponse>
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken = default);
}

public interface IQueryHandler<in TQuery, TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken = default);
}
