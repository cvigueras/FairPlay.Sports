using FairPlay.Sports.Domain.Challenges;
using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Challenges;

public sealed record ChallengeKitDto(
    string ColorPrimary,
    string ColorSecondary,
    string ShortsColor,
    KitPattern KitPattern,
    TeamKitSlot Slot)
{
    public static ChallengeKitDto FromDomain(KitColors colors, TeamKitSlot slot) =>
        new(colors.Primary, colors.Secondary, colors.ShortsColor, colors.Pattern, slot);
}
