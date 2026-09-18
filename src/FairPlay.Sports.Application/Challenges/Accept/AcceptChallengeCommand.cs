using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Challenges.Accept;

public sealed record AcceptChallengeCommand(Guid Id, Guid ActingUserId) : IRequest<Result<ChallengeDto>>;
