using FluentValidation;

namespace FairPlay.Sports.Application.Users.Register;

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        // Team is optional; reject only an explicitly empty guid.
        RuleFor(x => x.TeamId).NotEqual(Guid.Empty);
    }
}
