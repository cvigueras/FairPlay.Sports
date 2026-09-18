using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Challenges.Reject;

public sealed record RejectChallengeCommand(Guid Id, Guid ActingUserId) : IRequest<Result<ChallengeDto>>;
