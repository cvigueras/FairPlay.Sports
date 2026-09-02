using FluentValidation;

namespace FairPlay.Sports.Application.Teams.Activate;

public sealed class ActivateTeamValidator : AbstractValidator<ActivateTeamCommand>
{
    public ActivateTeamValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
