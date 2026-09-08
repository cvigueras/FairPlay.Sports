using FluentValidation;

namespace FairPlay.Sports.Application.Users.MoveToTeam;

public sealed class MoveUserToTeamValidator : AbstractValidator<MoveUserToTeamCommand>
{
    public MoveUserToTeamValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TeamId).NotEmpty();
    }
}
