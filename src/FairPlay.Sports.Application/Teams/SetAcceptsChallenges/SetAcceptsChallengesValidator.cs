using FluentValidation;

namespace FairPlay.Sports.Application.Teams.SetAcceptsChallenges;

public sealed class SetAcceptsChallengesValidator : AbstractValidator<SetAcceptsChallengesCommand>
{
    public SetAcceptsChallengesValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.ActingUserId).NotEmpty();
    }
}
