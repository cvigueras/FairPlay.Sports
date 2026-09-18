using FairPlay.Sports.Domain.Teams;

namespace FairPlay.Sports.Application.Challenges;

/// <summary>Who may send or respond to a challenge on a team's behalf, shared by Send/Accept/Reject.</summary>
internal static class ChallengeAuthorization
{
    public static bool CanActForTeam(TeamMemberRole role) =>
        role is TeamMemberRole.Delegate or TeamMemberRole.Coach or TeamMemberRole.President;
}
