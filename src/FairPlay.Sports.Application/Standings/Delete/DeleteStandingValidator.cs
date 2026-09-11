using FluentValidation;

namespace FairPlay.Sports.Application.Standings.Delete;

public sealed class DeleteStandingValidator : AbstractValidator<DeleteStandingCommand>
{
    public DeleteStandingValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
