using FluentValidation;

namespace FairPlay.Sports.Application.Auth.Login;

/// <summary>
/// Structural checks only. It deliberately does not validate the email <em>format</em>
/// or the password rules - a wrong shape and a wrong secret must both come back as the
/// same generic "incorrect" failure from the handler, with no hints.
/// </summary>
public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}
