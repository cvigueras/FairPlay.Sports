using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Standings.Create;

public sealed record CreateStandingCommand(
    Guid TeamId,
    int Points = default,
    int Played = default,
    int Won = default,
    int Drawn = default,
    int Lost = default,
    int GoalsFor = default,
    int GoalsAgainst = default) : IRequest<Result<StandingDto>>;
