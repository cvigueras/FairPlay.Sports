using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Domain.Challenges;
using MediatR;

namespace FairPlay.Sports.Application.Challenges.Send;

/// <param name="ChallengerKitPreference">
/// Which of the challenger's own kits to wear, whether it ends up playing home or away.
/// Ignored - and the automatic pick used instead - when it names a kit that team hasn't
/// configured.
/// </param>
/// <param name="ChallengedKitPreference">
/// Which of the challenged team's kits it should wear, chosen by the challenger. Ignored - and
/// the automatic clash-avoiding pick used instead - when it names a kit that team hasn't
/// configured.
/// </param>
public sealed record SendChallengeCommand(
    Guid ChallengerTeamId,
    Guid ChallengedTeamId,
    Guid VenueTeamId,
    DateTime MatchDate,
    string? Message,
    Guid ActingUserId,
    TeamKitSlot? ChallengerKitPreference = null,
    TeamKitSlot? ChallengedKitPreference = null) : IRequest<Result<ChallengeDto>>;
