using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Teams.SetAcceptsChallenges;

public sealed record SetAcceptsChallengesCommand(Guid TeamId, bool Accepts, Guid ActingUserId)
    : IRequest<Result<TeamDto>>;
