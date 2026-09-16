using FairPlay.Sports.Domain.Teams;
using FluentValidation;

namespace FairPlay.Sports.Application.Teams.JoinTeam;

public sealed class JoinTeamValidator : AbstractValidator<JoinTeamCommand>
{
    public JoinTeamValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role).IsInEnum();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(TeamMember.MaxDisplayNameLength);
    }
}
