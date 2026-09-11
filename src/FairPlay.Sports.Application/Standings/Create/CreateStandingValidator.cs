using FluentValidation;

namespace FairPlay.Sports.Application.Standings.Create;

public sealed class CreateStandingValidator : AbstractValidator<CreateStandingCommand>
{
    public CreateStandingValidator()
    {
        RuleFor(command => command.TeamId).NotEmpty();
        RuleFor(command => command.Points).GreaterThanOrEqualTo(0);
        RuleFor(command => command.Played).GreaterThanOrEqualTo(0);
        RuleFor(command => command.Won).GreaterThanOrEqualTo(0);
        RuleFor(command => command.Drawn).GreaterThanOrEqualTo(0);
        RuleFor(command => command.Lost).GreaterThanOrEqualTo(0);
        RuleFor(command => command.GoalsFor).GreaterThanOrEqualTo(0);
        RuleFor(command => command.GoalsAgainst).GreaterThanOrEqualTo(0);
        RuleFor(command => command)
            .Must(command => command.Won + command.Drawn + command.Lost == command.Played)
            .WithMessage("Played must equal won + drawn + lost.");
    }
}
