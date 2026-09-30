using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.SetVenueAvailable;

public sealed record SetVenueAvailableCommand(Guid TeamId, bool Available, Guid ActingUserId)
    : IRequest<Result<TeamDto>>;
