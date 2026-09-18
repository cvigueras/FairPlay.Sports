using FairPlay.Sports.Domain.Challenges;
using FluentValidation;

namespace FairPlay.Sports.Application.Challenges.Send;

public sealed class SendChallengeValidator : AbstractValidator<SendChallengeCommand>
{
    public SendChallengeValidator()
    {
        RuleFor(x => x.ChallengerTeamId).NotEmpty();
        RuleFor(x => x.ChallengedTeamId).NotEmpty();
        RuleFor(x => x.ActingUserId).NotEmpty();
        RuleFor(x => x.Message).MaximumLength(Challenge.MaxMessageLength);
        RuleFor(x => x.ChallengedTeamId)
            .NotEqual(x => x.ChallengerTeamId)
            .WithMessage("A team cannot challenge itself.");
    }
}
