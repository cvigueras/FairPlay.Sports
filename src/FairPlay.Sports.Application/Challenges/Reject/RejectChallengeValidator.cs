using FluentValidation;

namespace FairPlay.Sports.Application.Challenges.Reject;

public sealed class RejectChallengeValidator : AbstractValidator<RejectChallengeCommand>
{
    public RejectChallengeValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ActingUserId).NotEmpty();
    }
}
