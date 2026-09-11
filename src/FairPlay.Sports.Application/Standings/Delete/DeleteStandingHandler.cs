using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Standings.Delete;

public sealed class DeleteStandingHandler(IStandingRepository repository) : IRequestHandler<DeleteStandingCommand, Result>
{
    private readonly IStandingRepository _repository = repository;

    public async Task<Result> Handle(DeleteStandingCommand request, CancellationToken cancellationToken)
    {
        var standing = await _repository.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (standing is null)
            return Result.NotFound($"Standing '{request.Id}' was not found.");

        await _repository.RemoveAsync(standing, cancellationToken);

        return Result.Success();
    }
}
