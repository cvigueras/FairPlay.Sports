using FairPlay.Sports.Application.Users.Register;
using FairPlay.Sports.Domain.Teams;
using FairPlay.Sports.TestSupport.Users;

namespace FairPlay.Sports.Application.Tests.Users.Register;

[TestFixture]
public class RegisterUserValidatorTests
{
    private readonly RegisterUserValidator _validator = new();

    [Test]
    public void Validate_WithACompleteCommand_Passes()
    {
        Assert.That(_validator.Validate(UserMother.Command()).IsValid, Is.True);
    }

    [Test]
    public void Validate_WhenThePrivacyPolicyIsNotAccepted_Fails()
    {
        var result = _validator.Validate(UserMother.Command(acceptedPrivacyPolicy: false));

        Assert.That(result.Errors.Select(error => error.PropertyName),
            Does.Contain(nameof(RegisterUserCommand.AcceptedPrivacyPolicy)));
    }

    [Test]
    public void Validate_WhenTheMinimumAgeIsNotConfirmed_Fails()
    {
        var result = _validator.Validate(UserMother.Command(confirmedMinimumAge: false));

        Assert.That(result.Errors.Select(error => error.PropertyName),
            Does.Contain(nameof(RegisterUserCommand.ConfirmedMinimumAge)));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Validate_WhenFirstNameIsBlank_Fails(string firstName)
    {
        var result = _validator.Validate(UserMother.Command() with { FirstName = firstName });

        Assert.That(result.Errors.Select(error => error.PropertyName),
            Does.Contain(nameof(RegisterUserCommand.FirstName)));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Validate_WhenLastNameIsBlank_Fails(string lastName)
    {
        var result = _validator.Validate(UserMother.Command() with { LastName = lastName });

        Assert.That(result.Errors.Select(error => error.PropertyName),
            Does.Contain(nameof(RegisterUserCommand.LastName)));
    }

    [Test]
    public void Validate_WhenThePrimaryRoleIsNotAKnownRole_Fails()
    {
        var result = _validator.Validate(UserMother.Command() with { PrimaryRole = (TeamMemberRole)999 });

        Assert.That(result.Errors.Select(error => error.PropertyName),
            Does.Contain(nameof(RegisterUserCommand.PrimaryRole)));
    }
}
