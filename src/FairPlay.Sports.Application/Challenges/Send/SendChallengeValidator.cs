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
        RuleFor(x => x.MatchDate).NotEmpty();
        RuleFor(x => x.VenueTeamId)
            .Must((command, venueTeamId) => venueTeamId == command.ChallengerTeamId || venueTeamId == command.ChallengedTeamId)
            .WithMessage("The venue must belong to the challenger or the challenged team.");
        RuleFor(x => x.ChallengerKitPreference)
            .IsInEnum()
            .When(x => x.ChallengerKitPreference is not null);
    }
}
