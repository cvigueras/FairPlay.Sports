using FluentValidation;

namespace FairPlay.Sports.Application.Users.Register;

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PrimaryRole).IsInEnum();
        RuleFor(x => x.AcceptedPrivacyPolicy).Equal(true).WithMessage("The privacy policy must be accepted.");
        RuleFor(x => x.ConfirmedMinimumAge).Equal(true).WithMessage("You must be at least 14 years old to register.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}
