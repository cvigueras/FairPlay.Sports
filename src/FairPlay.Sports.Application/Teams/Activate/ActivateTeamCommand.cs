using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.Activate;

public sealed record ActivateTeamCommand(Guid Id) : IRequest<Result>;
