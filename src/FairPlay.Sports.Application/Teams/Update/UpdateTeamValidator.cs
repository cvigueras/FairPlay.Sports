using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FluentValidation;

namespace FairPlay.Sports.Application.Teams.Update;

public sealed class UpdateTeamValidator : AbstractValidator<UpdateTeamCommand>
{
    public UpdateTeamValidator(IClock clock)
    {
        RuleFor(x => x.Id).NotEmpty();
        this.AddTeamWriteRules(clock.UtcNow.Year);
    }
}
