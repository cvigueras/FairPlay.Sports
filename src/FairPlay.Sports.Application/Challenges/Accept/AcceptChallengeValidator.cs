using FluentValidation;

namespace FairPlay.Sports.Application.Challenges.Accept;

public sealed class AcceptChallengeValidator : AbstractValidator<AcceptChallengeCommand>
{
    public AcceptChallengeValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ActingUserId).NotEmpty();
    }
}
