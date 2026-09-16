using FluentValidation;

namespace FairPlay.Sports.Application.Teams.LeaveTeam;

public sealed class LeaveTeamValidator : AbstractValidator<LeaveTeamCommand>
{
    public LeaveTeamValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
