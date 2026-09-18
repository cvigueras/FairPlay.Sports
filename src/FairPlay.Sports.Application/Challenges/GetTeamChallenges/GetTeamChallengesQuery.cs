using FairPlay.Sports.Application.Common;
using MediatR;

namespace FairPlay.Sports.Application.Challenges.GetTeamChallenges;

/// <summary>Every challenge a team sent or received, newest first.</summary>
public sealed record GetTeamChallengesQuery(Guid TeamId) : IRequest<Result<IReadOnlyList<ChallengeDto>>>;
