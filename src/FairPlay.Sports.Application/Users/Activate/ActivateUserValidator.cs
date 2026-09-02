using FluentValidation;

namespace FairPlay.Sports.Application.Users.Activate;

public sealed class ActivateUserValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
