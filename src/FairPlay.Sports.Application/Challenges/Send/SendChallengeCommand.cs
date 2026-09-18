using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Domain.Challenges;
using MediatR;

namespace FairPlay.Sports.Application.Challenges.Send;

/// <param name="ChallengerKitPreference">
/// Which of the challenger's own kits to wear, when the challenger ends up playing away (the
/// challenged team's kit is never chosen this way). Ignored - and the automatic pick used
/// instead - when the challenger is home, or when it names a kit that team hasn't configured.
/// </param>
public sealed record SendChallengeCommand(
    Guid ChallengerTeamId,
    Guid ChallengedTeamId,
    Guid VenueTeamId,
    DateTime MatchDate,
    string? Message,
    Guid ActingUserId,
    TeamKitSlot? ChallengerKitPreference = null) : IRequest<Result<ChallengeDto>>;
