using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Challenges.Send;

public sealed record SendChallengeCommand(
    Guid ChallengerTeamId,
    Guid ChallengedTeamId,
    string? Message,
    Guid ActingUserId) : IRequest<Result<ChallengeDto>>;
