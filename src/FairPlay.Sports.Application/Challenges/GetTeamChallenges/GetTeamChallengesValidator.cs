using FluentValidation;

namespace FairPlay.Sports.Application.Challenges.GetTeamChallenges;

public sealed class GetTeamChallengesValidator : AbstractValidator<GetTeamChallengesQuery>
{
    public GetTeamChallengesValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
    }
}
