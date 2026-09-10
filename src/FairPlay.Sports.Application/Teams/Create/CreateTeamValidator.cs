using FairPlay.Sports.Application.Common;
using FairPlay.Sports.Application.Teams;
using FluentValidation;

namespace FairPlay.Sports.Application.Teams.Create;

public sealed class CreateTeamValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamValidator(IClock clock)
    {
        this.AddTeamWriteRules(clock.UtcNow.Year);
    }
}
