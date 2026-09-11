using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Standings.Delete;

public sealed record DeleteStandingCommand(Guid Id) : IRequest<Result>;
